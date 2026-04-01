using System.Text.Json.Serialization;

namespace TrackingApi.Contracts.Legacy;

public sealed class LegacyResponse<TData>
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("status")]
    public bool Status { get; init; }

    [JsonPropertyOrder(1)]
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyOrder(2)]
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TData? Data { get; init; }
}
