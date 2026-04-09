using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yuki_PC
{
    public class YukiMessage
    {
        [JsonPropertyName("protocol")]
        public string Protocol { get; set; } = "yuki/1.0";

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

        public static YukiMessage CreateHelloMessage(string deviceId, string deviceType, string[] capabilities = null)
        {
            var payload = new
            {
                device_id = deviceId,
                device_type = deviceType,
                capabilities = capabilities ?? Array.Empty<string>()
            };
            return new YukiMessage
            {
                Type = "hello",
                Id = Guid.NewGuid().ToString(),
                Payload = JsonDocument.Parse(JsonSerializer.Serialize(payload)).RootElement
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
    }
}