namespace TrackingApi.Domain.Entities;

public sealed class GesEcoOrdersTrackingEntity
{
    public int IdTracking { get; set; }
    public Guid CodPedido { get; set; }
    public string IdDelivery { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string CodigoZebra { get; set; } = string.Empty;
    public Guid? ShippingStatus { get; set; }
}
