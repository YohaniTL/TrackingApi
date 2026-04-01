using System.Text.Json.Serialization;

namespace TrackingApi.Contracts.Legacy;

public sealed class LabelResponseData
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("id_delivery")]
    public string IdDelivery { get; init; } = string.Empty;

    [JsonPropertyOrder(1)]
    [JsonPropertyName("id_pedido")]
    public string IdPedido { get; init; } = string.Empty;

    [JsonPropertyOrder(2)]
    [JsonPropertyName("codigoZebra")]
    public string CodigoZebra { get; init; } = string.Empty;
}
