// YukiClient.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Yuki_PC
{
    public class YukiClient : IDisposable
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _receiveTask;
        private Task _heartbeatTask;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        private string _serverAddress;
        private string _sessionId;
        private int _heartbeatInterval = 30;
        private string[] _enabledCapabilities = Array.Empty<string>();

        // Reconnection fields
        private CancellationTokenSource _reconnectCts;
        private int _reconnectAttempt;
        private bool _userInitiatedDisconnect;
        private bool _wasConnectedOnce;
        private bool _disposed;
        private readonly int _reconnectBackoffBase = 3; // seconds

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

        public YukiClient()
        {
            _webSocket = new ClientWebSocket();
        }

        public void SetCapabilities(string[] capabilities)
        {
            _enabledCapabilities = capabilities ?? Array.Empty<string>();
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

                    // Добавляем таймаут на handshake (10 секунд)
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

            try
            {
                // Отменяем все операции
                _cancellationTokenSource?.Cancel();

                await WaitForConnectionTasksAsync();

                // Принудительно закрываем WebSocket, даже если он в состоянии Connecting
                if (_webSocket != null)
                {
                    try
                    {
                        // Если WebSocket все еще открывается или коннектится
                        if (_webSocket.State == WebSocketState.Connecting ||
                            _webSocket.State == WebSocketState.Open ||
                            _webSocket.State == WebSocketState.CloseReceived ||
                            _webSocket.State == WebSocketState.CloseSent)
                        {
                            // Используем Abort для принудительного закрытия
                            _webSocket.Abort();
                            Log(Logger.LogLevel.INFO, "WebSocket aborted");
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

            try
            {
                _cancellationTokenSource?.Cancel();

                if (_webSocket != null)
                {
                    try
                    {
                        // Принудительное закрытие любым способом
                        _webSocket.Abort();
                    }
                    catch { }

                    try
                    {
                        _webSocket.Dispose();
                    }
                    catch { }

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
            Log(Logger.LogLevel.DEBUG, $"Sent status: {status}");
        }

        private static JsonElement CreateEmptyObject()
        {
            return JsonSerializer.SerializeToElement(new { });
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
                catch
                {
                    // ignore
                }

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
            catch
            {
                // ignore shutdown races
            }
            finally
            {
                _receiveTask = null;
                _heartbeatTask = null;
            }
        }

        private static bool TryGetString(JsonElement element, string propertyName, out string value)
        {
            value = null;

            if (element.ValueKind != JsonValueKind.Object)
                return false;

            if (!element.TryGetProperty(propertyName, out var prop))
                return false;

            if (prop.ValueKind == JsonValueKind.Null || prop.ValueKind == JsonValueKind.Undefined)
                return false;

            value = prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
            return !string.IsNullOrEmpty(value);
        }

        private static bool TryGetInt32(JsonElement element, string propertyName, out int value)
        {
            value = default;

            if (element.ValueKind != JsonValueKind.Object)
                return false;

            if (!element.TryGetProperty(propertyName, out var prop))
                return false;

            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out value))
                return true;

            if (prop.ValueKind == JsonValueKind.String &&
                int.TryParse(prop.GetString(), out value))
                return true;

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
                        StartHeartbeat(socket, token);
                        break;

                    case "command":
                        await HandleCommandAsync(root, body, msgId, token, socket);
                        break;

                    case "ping":
                        {
                            var pong = new YukiMessage
                            {
                                Type = "pong",
                                Id = msgId ?? Guid.NewGuid().ToString(),
                                Payload = CreateEmptyObject()
                            };
                            await SendMessageAsync(pong, token, socket);
                            break;
                        }

                    case "token_update":
                        {
                            string newToken = null;
                            string reason = "unknown";

                            if (TryGetString(body, "new_token", out var tokenValue))
                                newToken = tokenValue;

                            if (TryGetString(body, "reason", out var reasonValue))
                                reason = reasonValue;

                            if (!string.IsNullOrEmpty(newToken))
                            {
                                AuthToken = newToken;
                                Log(Logger.LogLevel.SUCCESS, $"Token updated by server. Reason: {reason}");
                                OnTokenUpdated?.Invoke(newToken);
                            }
                            break;
                        }

                    case "disconnect":
                        {
                            string reason = "unknown";
                            if (TryGetString(body, "reason", out var reasonValue))
                                reason = reasonValue;

                            Log(Logger.LogLevel.INFO, $"Server requested disconnect. Reason: {reason}");

                            _userInitiatedDisconnect = true;
                            _wasConnectedOnce = false;

                            // Сразу обновляем UI, чтобы статус не зависал на Connected
                            UpdateStatus(ConnectionStatus.Disconnected);

                            try
                            {
                                _cancellationTokenSource?.Cancel();
                            }
                            catch { }

                            if (socket.State == WebSocketState.Open ||
                                socket.State == WebSocketState.CloseReceived ||
                                socket.State == WebSocketState.CloseSent)
                            {
                                try
                                {
                                    await socket.CloseAsync(
                                        WebSocketCloseStatus.NormalClosure,
                                        "Disconnect by admin",
                                        CancellationToken.None);
                                }
                                catch (Exception ex)
                                {
                                    Log(Logger.LogLevel.ERROR, $"Error during disconnect close: {ex.Message}");
                                }
                            }
                            break;
                        }

                    case "reconnect":
                        {
                            Log(Logger.LogLevel.INFO, "Server requested reconnect");

                            _userInitiatedDisconnect = false;
                            UpdateStatus(ConnectionStatus.Reconnecting);

                            if (socket.State == WebSocketState.Open ||
                                socket.State == WebSocketState.CloseReceived ||
                                socket.State == WebSocketState.CloseSent)
                            {
                                try
                                {
                                    await socket.CloseAsync(
                                        WebSocketCloseStatus.NormalClosure,
                                        "Reconnect requested",
                                        CancellationToken.None);
                                }
                                catch (Exception ex)
                                {
                                    Log(Logger.LogLevel.ERROR, $"Error during reconnect close: {ex.Message}");
                                }
                            }
                            break;
                        }


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
                                    Payload = CreateEmptyObject()
                                };

                                await SendMessageAsync(ping, token, socket);
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }, token);
        }

        private async Task HandleCommandAsync(JsonElement root, JsonElement body, string msgId, CancellationToken token, ClientWebSocket socket)
        {
            string command = null;
            JsonElement payload = default;

            if (TryGetString(body, "command", out var cmd))
                command = cmd;

            if (body.ValueKind == JsonValueKind.Object &&
                body.TryGetProperty("params", out var paramsProp) &&
                paramsProp.ValueKind != JsonValueKind.Null &&
                paramsProp.ValueKind != JsonValueKind.Undefined)
            {
                payload = paramsProp;
            }
            else
            {
                payload = CreateEmptyObject();
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
                Log(Logger.LogLevel.WARN, $"Command '{command}' is disabled in client settings");
                var errorRes = YukiProtocol.CreateCommandResultMessage(msgId, false, null, "Command disabled by user");
                await SendMessageAsync(errorRes, token, socket);
                return;
            }

            var (success, result, error) = await CommandHandler.ExecuteAsync(command, payload);
            var resMsg = YukiProtocol.CreateCommandResultMessage(msgId, success, result, error);
            await SendMessageAsync(resMsg, token, socket);
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

            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch { }

            try
            {
                _sendLock.Dispose();
            }
            catch { }

            try
            {
                _webSocket?.Dispose();
            }
            catch { }

            try
            {
                _heartbeatTask?.Dispose();
            }
            catch { }
        }
    }
}