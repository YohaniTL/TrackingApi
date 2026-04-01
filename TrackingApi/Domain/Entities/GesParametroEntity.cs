namespace TrackingApi.Domain.Entities;

public sealed class GesParametroEntity
{
    public Guid CodParametro { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
