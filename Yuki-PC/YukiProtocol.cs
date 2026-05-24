using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yuki_PC
{
    public class YukiMessage
    {
        [JsonPropertyName("protocol")]
        public string Protocol { get; set; } = "yuki/1.1";

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }
    }

    public static class YukiProtocol
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static YukiMessage CreateHelloMessage(string deviceId, string deviceType,
            string[] capabilities = null, string authToken = null)
        {
            var payload = new Dictionary<string, object>
            {
                ["device_id"] = deviceId,
                ["device_type"] = deviceType,
                ["capabilities"] = capabilities ?? Array.Empty<string>()
            };
            if (!string.IsNullOrEmpty(authToken))
                payload["auth_token"] = authToken;

            string json = JsonSerializer.Serialize(payload);
            return new YukiMessage
            {
                Type = "hello",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(json).RootElement
            };
        }

        public static YukiMessage CreateStatusMessage(string deviceId, string status)
        {
            var payload = new { device_id = deviceId, status };
            return new YukiMessage
            {
                Type = "status",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(JsonSerializer.Serialize(payload)).RootElement
            };
        }

        public static YukiMessage CreateCommandResultMessage(string originalId, bool success, object result = null, string error = null)
        {
            var payload = new { success, result, error };
            return new YukiMessage
            {
                Type = "command_result",
                Id = originalId,
                Payload = JsonDocument.Parse(JsonSerializer.Serialize(payload)).RootElement
            };
        }

        // ==================== НОВЫЕ МЕТОДЫ ====================

        public static YukiMessage CreateExtendedStatusMessage(string deviceId, string status = null,
            string substatus = null, object details = null)
        {
            var payloadObj = new Dictionary<string, object>
            {
                ["device_id"] = deviceId
            };

            // Добавляем status только если он передан
            if (!string.IsNullOrEmpty(status))
                payloadObj["status"] = status;

            if (!string.IsNullOrEmpty(substatus))
                payloadObj["substatus"] = substatus;

            if (details != null)
                payloadObj["details"] = details;

            string json = JsonSerializer.Serialize(payloadObj);
            return new YukiMessage
            {
                Type = "extended_status",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(json).RootElement
            };
        }

        public static YukiMessage CreateMetricsMessage(string deviceId, object metrics)
        {
            var payloadObj = new Dictionary<string, object>
            {
                ["device_id"] = deviceId,
                ["metrics"] = metrics,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonSerializer.Serialize(payloadObj);
            return new YukiMessage
            {
                Type = "metrics",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(json).RootElement
            };
        }

        public static YukiMessage CreateDeviceToDeviceMessage(string fromDeviceId, string toDeviceId,
            string command, object payload = null, bool requireResponse = false)
        {
            var payloadObj = new Dictionary<string, object>
            {
                ["from_device_id"] = fromDeviceId,
                ["to_device_id"] = toDeviceId,
                ["command"] = command,
                ["payload"] = payload ?? new { },
                ["require_response"] = requireResponse,
                ["sent_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonSerializer.Serialize(payloadObj);
            return new YukiMessage
            {
                Type = "device_to_device",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(json).RootElement
            };
        }

        public static YukiMessage CreateDeviceResponseMessage(string originalId, string fromDeviceId,
            string toDeviceId, bool success, object result = null, string error = null)
        {
            var payloadObj = new Dictionary<string, object>
            {
                ["from_device_id"] = fromDeviceId,
                ["to_device_id"] = toDeviceId,
                ["success"] = success,
                ["result"] = result,
                ["error"] = error
            };

            string json = JsonSerializer.Serialize(payloadObj);
            return new YukiMessage
            {
                Type = "device_response",
                Id = originalId,
                Payload = JsonDocument.Parse(json).RootElement
            };
        }

        public static YukiMessage CreateDeviceBroadcastMessage(string fromDeviceId, string command,
            object payload = null, string[] deviceFilter = null)
        {
            var payloadObj = new Dictionary<string, object>
            {
                ["from_device_id"] = fromDeviceId,
                ["command"] = command,
                ["payload"] = payload ?? new { },
                ["device_filter"] = deviceFilter,
                ["sent_at"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string json = JsonSerializer.Serialize(payloadObj);
            return new YukiMessage
            {
                Type = "device_broadcast",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(json).RootElement
            };
        }
    }
}