using System.Text.Json.Serialization;

namespace OpenSenseMapAPI.Models
{
    public class CreateBoxApiResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public CreateBoxResponse? Data { get; set; }
    }

    public class CreateBoxResponse
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("exposure")]
        public string Exposure { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; } = string.Empty;

        [JsonPropertyName("currentLocation")]
        public CurrentLocation? CurrentLocation { get; set; }

        [JsonPropertyName("sensors")]
        public List<Sensor>? Sensors { get; set; }

        [JsonPropertyName("grouptag")]
        public List<string>? Grouptag { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }

    public class CurrentLocation
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("coordinates")]
        public List<double> Coordinates { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class Sensor
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonPropertyName("sensorType")]
        public string SensorType { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;
    }


}
