using System;
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
            Reconnecting   // новый статус
        }

        public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

        public event EventHandler<ConnectionStatus> OnStatusChanged;
        public event Action<string, Logger.LogLevel> OnLog;
        public event Action<string> OnDeviceIdUpdated;

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

            // Отменяем любые попытки переподключения
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
                    _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
                    await SendHelloAsync(token);
                }
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Connection failed: {ex.Message}");
                UpdateStatus(ConnectionStatus.Disconnected);
                await DisconnectAsync(userInitiated: false);
                // Не запускаем авто-переподключение, потому что это была ручная попытка подключения
            }
        }

        public async Task DisconnectAsync(bool userInitiated = true)
        {
            _userInitiatedDisconnect = userInitiated;
            CancelReconnection();

            try
            {
                _cancellationTokenSource?.Cancel();
                if (_webSocket?.State == WebSocketState.Open)
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
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

        private async Task SendHelloAsync(CancellationToken token)
        {
            var hello = YukiProtocol.CreateHelloMessage(DeviceId, "yuki-device-pc", _enabledCapabilities, AuthToken);
            await SendMessageAsync(hello, token);
            Log(Logger.LogLevel.INFO, $"Sent hello for device '{DeviceId}' with {_enabledCapabilities.Length} capabilities");
        }

        private async Task SendStatusAsync(string status, CancellationToken token)
        {
            var msg = YukiProtocol.CreateStatusMessage(DeviceId, status);
            await SendMessageAsync(msg, token);
            Log(Logger.LogLevel.DEBUG, $"Sent status: {status}");
        }

        private async Task SendMessageAsync(YukiMessage msg, CancellationToken token)
        {
            if (_webSocket.State != WebSocketState.Open) return;
            var json = JsonSerializer.Serialize(msg, YukiProtocol.SerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                while (_webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log(Logger.LogLevel.INFO, "Server closed connection");
                        break;
                    }
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessageAsync(json, token);
                }
            }
            catch (OperationCanceledException) { }
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
                // Соединение потеряно – запускаем переподключение, если это не было инициировано пользователем
                await HandleConnectionLost();
            }
        }

        private async Task HandleConnectionLost()
        {
            if (_userInitiatedDisconnect || _disposed)
                return;

            // Если мы никогда не были в Connected, не переподключаемся автоматически
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
            CancelReconnection(); // отменяем предыдущий цикл, если есть
            _reconnectCts = new CancellationTokenSource();
            var token = _reconnectCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && !_userInitiatedDisconnect && !_disposed)
                {
                    int delay = (int)Math.Pow(2, _reconnectAttempt) * _reconnectBackoffBase;
                    // Ограничим максимум 60 секундами, чтобы не ждать слишком долго
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
                // Сбрасываем старое соединение
                _cancellationTokenSource?.Cancel();
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                var uri = new Uri(_serverAddress + "/device");
                await _webSocket.ConnectAsync(uri, token);
                if (_webSocket.State != WebSocketState.Open)
                    return false;

                Log(Logger.LogLevel.SUCCESS, "WebSocket reconnected, sending hello...");
                UpdateStatus(ConnectionStatus.Handshaking);
                _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
                await SendHelloAsync(token);
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

        private async Task ProcessMessageAsync(string json, CancellationToken token)
        {
            try
            {
                var msg = JsonSerializer.Deserialize<YukiMessage>(json, YukiProtocol.SerializerOptions);
                if (msg == null) return;

                Log(Logger.LogLevel.DEBUG, $"Received: type={msg.Type}, id={msg.Id}");

                switch (msg.Type)
                {
                    case "welcome":
                        if (msg.Payload.TryGetProperty("session_id", out var sid))
                            _sessionId = sid.GetString();
                        if (msg.Payload.TryGetProperty("heartbeat_interval", out var hi) && hi.TryGetInt32(out var interval))
                            _heartbeatInterval = interval;
                        Log(Logger.LogLevel.SUCCESS, $"Welcome received. Heartbeat: {_heartbeatInterval}s");
                        _wasConnectedOnce = true;
                        UpdateStatus(ConnectionStatus.Connected);
                        await SendStatusAsync("online", token);
                        StartHeartbeat(token);
                        break;

                    case "command":
                        await HandleCommandAsync(msg, token);
                        break;

                    case "ping":
                        var pong = new YukiMessage { Type = "pong", Id = Guid.NewGuid().ToString(), Payload = new JsonElement() };
                        await SendMessageAsync(pong, token);
                        break;

                    default:
                        Log(Logger.LogLevel.WARN, $"Unhandled message type: {msg.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log(Logger.LogLevel.ERROR, $"Message processing error: {ex.Message}");
            }
        }

        private void StartHeartbeat(CancellationToken token)
        {
            _heartbeatTask = Task.Run(async () =>
            {
                while (_webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    await Task.Delay(_heartbeatInterval * 1000, token);
                    if (_webSocket.State == WebSocketState.Open)
                    {
                        try
                        {
                            var ping = new YukiMessage { Type = "ping", Id = Guid.NewGuid().ToString(), Payload = new JsonElement() };
                            await SendMessageAsync(ping, token);
                        }
                        catch { break; }
                    }
                }
            }, token);
        }

        private async Task HandleCommandAsync(YukiMessage msg, CancellationToken token)
        {
            string command = null;
            JsonElement payload = default;
            if (msg.Payload.TryGetProperty("command", out var cmdProp))
                command = cmdProp.GetString();
            if (msg.Payload.TryGetProperty("params", out var paramsProp))
                payload = paramsProp;

            Log(Logger.LogLevel.INFO, $"Command received: {command}");

            if (!_enabledCapabilities.Contains(command, StringComparer.OrdinalIgnoreCase))
            {
                Log(Logger.LogLevel.WARN, $"Command '{command}' is disabled in client settings");
                var errorRes = YukiProtocol.CreateCommandResultMessage(msg.Id, false, null, "Command disabled by user");
                await SendMessageAsync(errorRes, token);
                return;
            }

            var (success, result, error) = await CommandHandler.ExecuteAsync(command, payload);
            var resMsg = YukiProtocol.CreateCommandResultMessage(msg.Id, success, result, error);
            await SendMessageAsync(resMsg, token);
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
        }

        public void Dispose()
        {
            _disposed = true;
            CancelReconnection();
            _cancellationTokenSource?.Cancel();
            _webSocket?.Dispose();
            _heartbeatTask?.Dispose();
        }
    }
}