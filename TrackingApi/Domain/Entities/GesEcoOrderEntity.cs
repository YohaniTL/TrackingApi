namespace TrackingApi.Domain.Entities;

public sealed class GesEcoOrderEntity
{
    public int Id { get; set; }
    public Guid CodPedido { get; set; }
    public string? ShippingStatus { get; set; }
    public string? IdDelivery { get; set; }
}
