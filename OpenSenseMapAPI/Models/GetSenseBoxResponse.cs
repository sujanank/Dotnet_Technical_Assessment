using System.Text.Json.Serialization;

namespace OpenSenseMapAPI.Models;

public class GetSenseBoxResponse
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("exposure")]
    public string Exposure { get; set; } = string.Empty;

    [JsonPropertyName("grouptag")]
    public List<string>? Grouptag { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("currentLocation")]
    public CurrentLocation? CurrentLocation { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sensors")]
    public List<SensorWithMeasurement>? Sensors { get; set; }

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = string.Empty;
}

public class SensorWithMeasurement
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("sensorType")]
    public string SensorType { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("lastMeasurement")]
    public LastMeasurement? LastMeasurement { get; set; }
}

public class LastMeasurement
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;
}
