using Microsoft.AspNetCore.Mvc;

namespace TrackingApi.Contracts.Requests;

public sealed class LegacyCodigoRequest
{
    [FromForm(Name = "codigo")]
    public string? Codigo { get; init; }
}
