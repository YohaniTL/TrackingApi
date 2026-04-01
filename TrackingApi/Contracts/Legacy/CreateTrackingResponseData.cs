using System.Text.Json.Serialization;

namespace TrackingApi.Contracts.Legacy;

public sealed class CreateTrackingResponseData
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
