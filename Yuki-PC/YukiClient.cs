using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;

namespace Yuki_PC
{
    public class YukiClient : IDisposable
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _receiveTask;
        private Task _heartbeatTask;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private Timer _metricsTimer;
        private Timer _statusTimer;

        private string _serverAddress;
        private string _sessionId;
        private int _heartbeatInterval = 30;
        private string[] _enabledCapabilities = Array.Empty<string>();
        private string _currentSubstatus = "idle";
        private Dictionary<string, object> _currentMetrics = new Dictionary<string, object>();

        // Reconnection fields
        private CancellationTokenSource _reconnectCts;
        private int _reconnectAttempt;
        private bool _userInitiatedDisconnect;
        private bool _wasConnectedOnce;
        private bool _disposed;
        private readonly int _reconnectBackoffBase = 3;

        public string DeviceId { get; set; }
        public string AuthToken { get; set; }

        public enum ConnectionStatus
        {
            Disconnected,
            Connecting,
            Handshaking,
            Connected,
            Reconnecting
        }

        public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

        public event EventHandler<ConnectionStatus> OnStatusChanged;
        public event Action<string, Logger.LogLevel> OnLog;
        public event Action<string> OnDeviceIdUpdated;
        public event Action<string> OnTokenUpdated;
        public event Action<string, string, object> OnDeviceCommand;
        public event Action<string, object> OnDeviceBroadcast;
        public event Func<string, object, Task<object>> OnDeviceCommandAsync;

        public YukiClient()
        {
            _webSocket = new ClientWebSocket();
        }

        public void SetCapabilities(string[] capabilities)
        {
            _enabledCapabilities = capabilities ?? Array.Empty<string>();
        }

        public void SetExtendedStatus(string substatus, object details = null)
        {
            _currentSubstatus = substatus;
            if (Status == ConnectionStatus.Connected && _webSocket.State == WebSocketState.Open)
            {
                _ = SendExtendedStatusAsync(substatus, details);
            }
        }

        public void UpdateMetrics(Dictionary<string, object> metrics)
        {
            foreach (var kv in metrics)
                _currentMetrics[kv.Key] = kv.Value;

            if (Status == ConnectionStatus.Connected && _webSocket.State == WebSocketState.Open)
            {
                _ = SendMetricsAsync();
            }
        }

        public async Task SendToDeviceAsync(string targetDeviceId, string command,
            object payload = null, bool requireResponse = false)
        {
            if (Status != ConnectionStatus.Connected)
            {
                Log(Logger.LogLevel.WARN, "Cannot send to device: not connected");
                return;
            }

            var msg = YukiProtocol.CreateDeviceToDeviceMessage(
                DeviceId, targetDeviceId, command, payload, requireResponse);
            await SendMessageAsync(msg, _cancellationTokenSource.Token, _webSocket);
            Log(Logger.LogLevel.INFO, $"Sent command to {targetDeviceId}: {command}");
        }

        public async Task BroadcastToDevicesAsync(string command, object payload = null,
            string[] deviceFilter = null)
        {
            if (Status != ConnectionStatus.Connected)
            {
                Log(Logger.LogLevel.WARN, "Cannot broadcast: not connected");
                return;
            }

            var broadcastMsg = YukiProtocol.CreateDeviceBroadcastMessage(DeviceId, command, payload, deviceFilter);
            await SendMessageAsync(broadcastMsg, _cancellationTokenSource.Token, _webSocket);
            Log(Logger.LogLevel.INFO, $"Broadcast '{command}' to {(deviceFilter?.Length ?? 0)} devices");
        }

        public void StartMetricsReporting(int intervalSeconds = 60)
        {
            _metricsTimer?.Dispose();
            _metricsTimer = new Timer(async _ =>
            {
                if (Status == ConnectionStatus.Connected && _webSocket.State == WebSocketState.Open)
                {
                    await CollectAndSendMetrics();
                }
            }, null, intervalSeconds * 1000, intervalSeconds * 1000);

            Log(Logger.LogLevel.INFO, $"Metrics reporting started (interval: {intervalSeconds}s)");
        }

        public void StopMetricsReporting()
        {
            _metricsTimer?.Dispose();
            _metricsTimer = null;
            Log(Logger.LogLevel.INFO, "Metrics reporting stopped");
        }

        public void StartPeriodicStatus(int intervalSeconds = 30)
        {
            _statusTimer?.Dispose();
            _statusTimer = new Timer(async _ =>
            {
                if (Status == ConnectionStatus.Connected && _webSocket.State == WebSocketState.Open)
                {
                    await SendStatusAsync("online", _cancellationTokenSource.Token, _webSocket);
                    // Убираем extended status отсюда
                    // await SendExtendedStatusAsync(_currentSubstatus, null);
                }
            }, null, intervalSeconds * 1000, intervalSeconds * 1000);

            Log(Logger.LogLevel.INFO, $"Periodic status started (interval: {intervalSeconds}s)");
        }

        public void StopPeriodicStatus()
        {
            _statusTimer?.Dispose();
            _statusTimer = null;
            Log(Logger.LogLevel.INFO, "Periodic status stopped");
        }

        public async Task ConnectAsync(string serverAddress)
        {
            if (Status != ConnectionStatus.Disconnected && Status != ConnectionStatus.Reconnecting)
                await DisconnectAsync(userInitiated: true);

            CancelReconnection();

            _serverAddress = serverAddress;
            _userInitiatedDisconnect = false;
            _wasConnectedOnce = false;
            _reconnectAttempt = 0;

            UpdateStatus(ConnectionStatus.Connecting);
            Log(Logger.LogLevel.INFO, $"Connecting to {serverAddress}...");

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                var uri = new Uri(serverAddress + "/device");
                await _webSocket.ConnectAsync(uri, token);

                if (_webSocket.State == WebSocketState.Open)
                {
                    Log(Logger.LogLevel.SUCCESS, "WebSocket connected, sending hello...");
                    UpdateStatus(ConnectionStatus.Handshaking);

                    var socket = _webSocket;
                    _receiveTask = Task.Run(() => ReceiveLoopAsync(socket, token), token);

                    await SendHelloAsync(token, socket);

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(10000);
                        if (Status == ConnectionStatus.Handshaking)
                        {
                            Log(Logger.LogLevel.WARN, "Handshake timeout, forcing disconnect");
                            ForceDisconnect();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Connection failed: {ex.Message}");
                UpdateStatus(ConnectionStatus.Disconnected);
                await DisconnectAsync(userInitiated: false);
            }
        }

        public async Task DisconnectAsync(bool userInitiated = true)
        {
            _userInitiatedDisconnect = userInitiated;
            CancelReconnection();
            StopMetricsReporting();
            StopPeriodicStatus();

            try
            {
                _cancellationTokenSource?.Cancel();
                await WaitForConnectionTasksAsync();

                if (_webSocket != null)
                {
                    try
                    {
                        if (_webSocket.State == WebSocketState.Connecting ||
                            _webSocket.State == WebSocketState.Open ||
                            _webSocket.State == WebSocketState.CloseReceived ||
                            _webSocket.State == WebSocketState.CloseSent)
                        {
                            _webSocket.Abort();
                        }
                        _webSocket.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log(Logger.LogLevel.ERROR, $"Error during socket dispose: {ex.Message}");
                    }
                    _webSocket = new ClientWebSocket();
                }
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Disconnect error: {ex.Message}");
            }
            finally
            {
                UpdateStatus(ConnectionStatus.Disconnected);
                Log(Logger.LogLevel.INFO, "Disconnected from server");
            }
        }

        public void ForceDisconnect()
        {
            Log(Logger.LogLevel.INFO, "Force disconnecting...");
            _userInitiatedDisconnect = true;
            CancelReconnection();
            StopMetricsReporting();
            StopPeriodicStatus();

            try
            {
                _cancellationTokenSource?.Cancel();
                if (_webSocket != null)
                {
                    try { _webSocket.Abort(); } catch { }
                    try { _webSocket.Dispose(); } catch { }
                    _webSocket = new ClientWebSocket();
                }
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Force disconnect error: {ex.Message}");
            }
            finally
            {
                UpdateStatus(ConnectionStatus.Disconnected);
            }
        }

        private async Task SendHelloAsync(CancellationToken token, ClientWebSocket socket)
        {
            var hello = YukiProtocol.CreateHelloMessage(DeviceId, "yuki-device-pc", _enabledCapabilities, AuthToken);
            await SendMessageAsync(hello, token, socket);
            Log(Logger.LogLevel.INFO, $"Sent hello for device '{DeviceId}' with {_enabledCapabilities.Length} capabilities");
        }

        private async Task SendStatusAsync(string status, CancellationToken token, ClientWebSocket socket)
        {
            var msg = YukiProtocol.CreateStatusMessage(DeviceId, status);
            await SendMessageAsync(msg, token, socket);
        }

        private async Task SendExtendedStatusAsync(string substatus, object details)
        {
            // Убираем status из сообщения, так как он уже известен ядру
            var msg = YukiProtocol.CreateExtendedStatusMessage(DeviceId, null, substatus, details);
            await SendMessageAsync(msg, _cancellationTokenSource.Token, _webSocket);
            Log(Logger.LogLevel.DEBUG, $"Extended status sent: {substatus}");
        }

        private async Task SendMetricsAsync()
        {
            var metrics = new Dictionary<string, object>(_currentMetrics);
            metrics["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Убираем статус из метрик
            var msg = YukiProtocol.CreateMetricsMessage(DeviceId, metrics);
            await SendMessageAsync(msg, _cancellationTokenSource.Token, _webSocket);
            Log(Logger.LogLevel.DEBUG, $"Metrics sent: {_currentMetrics.Count} values");
        }

        private async Task CollectAndSendMetrics()
        {
            try
            {
                var metrics = new Dictionary<string, object>();

                // CPU Usage
                try
                {
                    var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    metrics["cpu"] = Math.Round(cpuCounter.NextValue(), 1);
                    await Task.Delay(100);
                    metrics["cpu"] = Math.Round(cpuCounter.NextValue(), 1);
                    cpuCounter.Dispose();
                }
                catch (Exception ex)
                {
                    Log(Logger.LogLevel.DEBUG, $"CPU counter failed: {ex.Message}");
                    metrics["cpu"] = 0;
                }

                // Memory
                try
                {
                    var computerInfo = new ComputerInfo();
                    var availableMemory = computerInfo.AvailablePhysicalMemory;
                    var totalMemory = computerInfo.TotalPhysicalMemory;
                    metrics["memory_percent"] = Math.Round((totalMemory - availableMemory) * 100.0 / totalMemory, 1);
                    metrics["memory_used_gb"] = Math.Round((totalMemory - availableMemory) / 1024.0 / 1024.0 / 1024.0, 1);
                    metrics["memory_total_gb"] = Math.Round(totalMemory / 1024.0 / 1024.0 / 1024.0, 1);
                }
                catch (Exception ex)
                {
                    Log(Logger.LogLevel.DEBUG, $"Memory collection failed: {ex.Message}");
                }

                // Disk
                try
                {
                    var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
                    metrics["disk_percent"] = Math.Round((drive.TotalSize - drive.AvailableFreeSpace) * 100.0 / drive.TotalSize, 1);
                    metrics["disk_used_gb"] = Math.Round((drive.TotalSize - drive.AvailableFreeSpace) / 1024.0 / 1024.0 / 1024.0, 1);
                    metrics["disk_total_gb"] = Math.Round(drive.TotalSize / 1024.0 / 1024.0 / 1024.0, 1);
                }
                catch (Exception ex)
                {
                    Log(Logger.LogLevel.DEBUG, $"Disk collection failed: {ex.Message}");
                }

                // Uptime
                metrics["uptime_seconds"] = (int)(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;

                // System Info
                metrics["os"] = Environment.OSVersion.ToString();
                metrics["hostname"] = Environment.MachineName;
                metrics["username"] = Environment.UserName;

                UpdateMetrics(metrics);
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Failed to collect metrics: {ex.Message}");
            }
        }

        private async Task SendMessageAsync(YukiMessage msg, CancellationToken token, ClientWebSocket socket = null)
        {
            socket ??= _webSocket;

            if (socket == null || socket.State != WebSocketState.Open)
                return;

            var json = JsonSerializer.Serialize(msg, YukiProtocol.SerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _sendLock.WaitAsync(token);
            try
            {
                if (socket.State != WebSocketState.Open)
                    return;

                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    token);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken token)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Log(Logger.LogLevel.INFO, "Server closed connection");
                            return;
                        }

                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    var json = Encoding.UTF8.GetString(ms.ToArray());
                    await ProcessMessageAsync(json, token, socket);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (WebSocketException ex)
            {
                Log(Logger.LogLevel.ERROR, $"WebSocket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Receive error: {ex.Message}");
            }
            finally
            {
                await HandleConnectionLost();
            }
        }

        private async Task HandleConnectionLost()
        {
            if (_userInitiatedDisconnect || _disposed)
                return;

            if (!_wasConnectedOnce)
            {
                UpdateStatus(ConnectionStatus.Disconnected);
                return;
            }

            Log(Logger.LogLevel.WARN, "Connection lost, starting reconnection process...");
            UpdateStatus(ConnectionStatus.Reconnecting);
            StartReconnection();
        }

        private void StartReconnection()
        {
            CancelReconnection();
            _reconnectCts = new CancellationTokenSource();
            var token = _reconnectCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && !_userInitiatedDisconnect && !_disposed)
                {
                    int delay = (int)Math.Pow(2, _reconnectAttempt) * _reconnectBackoffBase;
                    if (delay > 60) delay = 60;

                    Log(Logger.LogLevel.INFO, $"Reconnection attempt {_reconnectAttempt + 1} in {delay} seconds...");
                    try
                    {
                        await Task.Delay(delay * 1000, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (token.IsCancellationRequested || _userInitiatedDisconnect || _disposed)
                        break;

                    Log(Logger.LogLevel.INFO, $"Attempting to reconnect to {_serverAddress}...");
                    bool success = await TryReconnectAsync();
                    if (success)
                    {
                        _reconnectAttempt = 0;
                        Log(Logger.LogLevel.SUCCESS, "Reconnection successful");
                        break;
                    }
                    else
                    {
                        _reconnectAttempt++;
                        Log(Logger.LogLevel.WARN, $"Reconnection attempt {_reconnectAttempt} failed");
                    }
                }
            }, token);
        }

        private async Task<bool> TryReconnectAsync()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                await WaitForConnectionTasksAsync();

                try
                {
                    if (_webSocket != null &&
                        (_webSocket.State == WebSocketState.Open ||
                         _webSocket.State == WebSocketState.CloseReceived ||
                         _webSocket.State == WebSocketState.CloseSent))
                    {
                        _webSocket.Abort();
                    }
                }
                catch { }

                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;
                var socket = _webSocket;

                var uri = new Uri(_serverAddress + "/device");
                await socket.ConnectAsync(uri, token);

                if (socket.State != WebSocketState.Open)
                    return false;

                Log(Logger.LogLevel.SUCCESS, "WebSocket reconnected, sending hello...");
                UpdateStatus(ConnectionStatus.Handshaking);

                _receiveTask = Task.Run(() => ReceiveLoopAsync(socket, token), token);
                await SendHelloAsync(token, socket);
                return true;
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Reconnection error: {ex.Message}");
                return false;
            }
        }

        private void CancelReconnection()
        {
            _reconnectCts?.Cancel();
            _reconnectCts?.Dispose();
            _reconnectCts = null;
        }

        private async Task WaitForConnectionTasksAsync()
        {
            var tasks = new List<Task>();

            if (_receiveTask != null)
                tasks.Add(_receiveTask);
            if (_heartbeatTask != null)
                tasks.Add(_heartbeatTask);

            if (tasks.Count == 0)
                return;

            try
            {
                var all = Task.WhenAll(tasks);
                var completed = await Task.WhenAny(all, Task.Delay(2000));
                if (completed == all)
                {
                    try { await all; } catch { }
                }
            }
            catch { }
            finally
            {
                _receiveTask = null;
                _heartbeatTask = null;
            }
        }

        private static bool TryGetString(JsonElement element, string propertyName, out string value)
        {
            value = null;
            if (element.ValueKind != JsonValueKind.Object) return false;
            if (!element.TryGetProperty(propertyName, out var prop)) return false;
            if (prop.ValueKind == JsonValueKind.Null || prop.ValueKind == JsonValueKind.Undefined) return false;
            value = prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
            return !string.IsNullOrEmpty(value);
        }

        private static bool TryGetInt32(JsonElement element, string propertyName, out int value)
        {
            value = default;
            if (element.ValueKind != JsonValueKind.Object) return false;
            if (!element.TryGetProperty(propertyName, out var prop)) return false;
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out value)) return true;
            if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out value)) return true;
            return false;
        }

        private static JsonElement GetBody(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("payload", out var payload) &&
                payload.ValueKind != JsonValueKind.Null &&
                payload.ValueKind != JsonValueKind.Undefined)
            {
                return payload;
            }
            return root;
        }

        private async Task ProcessMessageAsync(string json, CancellationToken token, ClientWebSocket socket)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var body = GetBody(root);

                if (!TryGetString(root, "type", out var type))
                {
                    Log(Logger.LogLevel.WARN, $"Invalid message without type: {json}");
                    return;
                }

                var hasId = TryGetString(root, "id", out var msgId);
                Log(Logger.LogLevel.DEBUG, $"Received: type={type}, id={(hasId ? msgId : "n/a")}");

                switch (type)
                {
                    case "welcome":
                        if (TryGetString(body, "session_id", out var sid))
                            _sessionId = sid;
                        if (TryGetInt32(body, "heartbeat_interval", out var interval))
                            _heartbeatInterval = interval;
                        Log(Logger.LogLevel.SUCCESS, $"Welcome received. Heartbeat: {_heartbeatInterval}s");
                        _wasConnectedOnce = true;
                        UpdateStatus(ConnectionStatus.Connected);
                        await SendStatusAsync("online", token, socket);
                        await SendExtendedStatusAsync(_currentSubstatus, null);
                        StartHeartbeat(socket, token);
                        break;

                    case "command":
                        await HandleCommandAsync(root, body, msgId, token, socket);
                        break;

                    case "device_command":
                        await HandleDeviceCommandAsync(root, body, msgId, token, socket);
                        break;

                    case "device_broadcast":
                        await HandleDeviceBroadcastAsync(root, body, token, socket);
                        break;

                    case "device_response":
                        HandleDeviceResponse(root, body);
                        break;

                    case "ping":
                        var pong = new YukiMessage
                        {
                            Type = "pong",
                            Id = msgId ?? Guid.NewGuid().ToString(),
                            Payload = JsonDocument.Parse("{}").RootElement
                        };
                        await SendMessageAsync(pong, token, socket);
                        break;

                    case "token_update":
                        string newToken = null;
                        if (TryGetString(body, "new_token", out var tokenValue))
                            newToken = tokenValue;
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            AuthToken = newToken;
                            Log(Logger.LogLevel.SUCCESS, $"Token updated by server");
                            OnTokenUpdated?.Invoke(newToken);
                        }
                        break;

                    case "disconnect":
                        Log(Logger.LogLevel.INFO, "Server requested disconnect");
                        _userInitiatedDisconnect = true;
                        _wasConnectedOnce = false;
                        UpdateStatus(ConnectionStatus.Disconnected);
                        try { _cancellationTokenSource?.Cancel(); } catch { }
                        if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived || socket.State == WebSocketState.CloseSent)
                        {
                            try { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect by admin", CancellationToken.None); } catch { }
                        }
                        break;

                    default:
                        Log(Logger.LogLevel.WARN, $"Unhandled message type: {type}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Log(Logger.LogLevel.ERROR, $"Invalid JSON message: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Message processing error: {ex.Message}");
            }
        }

        private async Task HandleDeviceCommandAsync(JsonElement root, JsonElement body,
            string msgId, CancellationToken token, ClientWebSocket socket)
        {
            string fromDeviceId = null;
            string command = null;
            JsonElement payload = default;
            bool requireResponse = false;

            if (TryGetString(body, "from_device_id", out var fromId))
                fromDeviceId = fromId;
            if (TryGetString(body, "command", out var cmd))
                command = cmd;
            if (body.TryGetProperty("payload", out var payProp))
                payload = payProp;
            if (body.TryGetProperty("require_response", out var respProp) && respProp.ValueKind == JsonValueKind.True)
                requireResponse = true;

            Log(Logger.LogLevel.INFO, $"Device command from {fromDeviceId}: {command}");

            object result = null;
            bool success = true;
            string error = null;

            if (OnDeviceCommandAsync != null)
            {
                try
                {
                    result = await OnDeviceCommandAsync(command, payload);
                }
                catch (Exception ex)
                {
                    success = false;
                    error = ex.Message;
                }
            }
            else if (OnDeviceCommand != null)
            {
                OnDeviceCommand?.Invoke(fromDeviceId, command, payload);  // ✅ 3 аргумента: fromDeviceId, command, payload
            }
            else
            {
                success = false;
                error = "No command handler registered";
            }

            if (requireResponse)
            {
                var responseMsg = YukiProtocol.CreateDeviceResponseMessage(
                    msgId, DeviceId, fromDeviceId, success, result, error);
                await SendMessageAsync(responseMsg, token, socket);
            }
        }

        private async Task HandleDeviceBroadcastAsync(JsonElement root, JsonElement body,
            CancellationToken token, ClientWebSocket socket)
        {
            string fromDeviceId = null;
            string command = null;
            JsonElement payload = default;

            if (TryGetString(body, "from_device_id", out var fromId))
                fromDeviceId = fromId;
            if (TryGetString(body, "command", out var cmd))
                command = cmd;
            if (body.TryGetProperty("payload", out var payProp))
                payload = payProp;

            Log(Logger.LogLevel.INFO, $"Device broadcast from {fromDeviceId}: {command}");
            OnDeviceBroadcast?.Invoke(command, payload);
        }

        private void HandleDeviceResponse(JsonElement root, JsonElement body)
        {
            bool success = false;
            string error = null;

            if (body.TryGetProperty("success", out var succProp))
                success = succProp.ValueKind == JsonValueKind.True;
            if (body.TryGetProperty("error", out var errProp))
                error = errProp.GetString();

            Log(Logger.LogLevel.INFO, $"Device response received: success={success}, error={error}");
        }

        private async Task HandleCommandAsync(JsonElement root, JsonElement body, string msgId, CancellationToken token, ClientWebSocket socket)
        {
            string command = null;
            JsonElement payload = default;

            if (TryGetString(body, "command", out var cmd))
                command = cmd;

            if (body.ValueKind == JsonValueKind.Object &&
                body.TryGetProperty("params", out var paramsProp) &&
                paramsProp.ValueKind != JsonValueKind.Null)
            {
                payload = paramsProp;
            }
            else
            {
                payload = JsonDocument.Parse("{}").RootElement;
            }

            Log(Logger.LogLevel.INFO, $"Command received: {command}");

            if (string.IsNullOrWhiteSpace(command))
            {
                var errorRes = YukiProtocol.CreateCommandResultMessage(msgId, false, null, "Missing command name");
                await SendMessageAsync(errorRes, token, socket);
                return;
            }

            if (!_enabledCapabilities.Contains(command, StringComparer.OrdinalIgnoreCase))
            {
                Log(Logger.LogLevel.WARN, $"Command '{command}' is disabled");
                var errorRes = YukiProtocol.CreateCommandResultMessage(msgId, false, null, "Command disabled by user");
                await SendMessageAsync(errorRes, token, socket);
                return;
            }

            var (success, result, error) = await CommandHandler.ExecuteAsync(command, payload);
            var resMsg = YukiProtocol.CreateCommandResultMessage(msgId, success, result, error);
            await SendMessageAsync(resMsg, token, socket);
        }

        private void StartHeartbeat(ClientWebSocket socket, CancellationToken token)
        {
            _heartbeatTask = Task.Run(async () =>
            {
                try
                {
                    while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                    {
                        await Task.Delay(_heartbeatInterval * 1000, token);

                        if (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                        {
                            try
                            {
                                var ping = new YukiMessage
                                {
                                    Type = "ping",
                                    Id = Guid.NewGuid().ToString(),
                                    Payload = JsonDocument.Parse("{}").RootElement
                                };
                                await SendMessageAsync(ping, token, socket);
                            }
                            catch { break; }
                        }
                    }
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        private void UpdateStatus(ConnectionStatus newStatus)
        {
            if (Status != newStatus)
            {
                Status = newStatus;
                OnStatusChanged?.Invoke(this, newStatus);
            }
        }

        private void Log(Logger.LogLevel level, string message)
        {
            switch (level)
            {
                case Logger.LogLevel.INFO: Logger.Info(message); break;
                case Logger.LogLevel.WARN: Logger.Warning(message); break;
                case Logger.LogLevel.ERROR: Logger.Error(message); break;
                case Logger.LogLevel.DEBUG: Logger.Debug(message); break;
                case Logger.LogLevel.SUCCESS: Logger.Success(message); break;
            }
            OnLog?.Invoke(message, level);
        }

        public void Dispose()
        {
            _disposed = true;
            CancelReconnection();
            StopMetricsReporting();
            StopPeriodicStatus();

            try { _cancellationTokenSource?.Cancel(); } catch { }
            try { _sendLock.Dispose(); } catch { }
            try { _webSocket?.Dispose(); } catch { }
            try { _heartbeatTask?.Dispose(); } catch { }
        }
    }
}