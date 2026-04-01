using Microsoft.AspNetCore.Mvc;

namespace TrackingApi.Contracts.Requests;

public sealed class LegacyLoginRequest
{
    [FromForm(Name = "username")]
    public string? Username { get; init; }

    [FromForm(Name = "password")]
    public string? Password { get; init; }
}
