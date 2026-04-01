using Microsoft.AspNetCore.Mvc;

namespace TrackingApi.Contracts.Requests;

public sealed class LegacyOrdersTrackingRequest
{
    [FromForm(Name = "id_traking")]
    public string? IdTraking { get; init; }

    [FromForm(Name = "cod_pedido")]
    public string? CodPedido { get; init; }

    [FromForm(Name = "id_delivery")]
    public string? IdDelivery { get; init; }

    [FromForm(Name = "nombreArchivo")]
    public string? NombreArchivo { get; init; }

    [FromForm(Name = "codigoZebra")]
    public string? CodigoZebra { get; init; }

    [FromForm(Name = "nombres")]
    public string? Nombres { get; init; }

    [FromForm(Name = "apellidos")]
    public string? Apellidos { get; init; }

    [FromForm(Name = "commune_code")]
    public string? CommuneCode { get; init; }
}
