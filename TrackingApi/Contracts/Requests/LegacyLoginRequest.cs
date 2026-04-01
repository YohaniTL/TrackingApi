using Microsoft.AspNetCore.Mvc;

namespace TrackingApi.Contracts.Requests;

public sealed class LegacyLoginRequest
{
    [FromForm(Name = "email")]
    public string? Email { get; init; }

    [FromForm(Name = "password")]
    public string? Password { get; init; }
}
