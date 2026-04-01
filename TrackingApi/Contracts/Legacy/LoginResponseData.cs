using System.Text.Json.Serialization;

namespace TrackingApi.Contracts.Legacy;

public sealed class LoginResponseData
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    [JsonPropertyOrder(1)]
    [JsonPropertyName("type")]
    public string Type { get; init; } = "Bearer";

    [JsonPropertyOrder(2)]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyOrder(3)]
    [JsonPropertyName("expired_at")]
    public string ExpiredAt { get; init; } = string.Empty;
}
