using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrackingApi.Contracts.Legacy;
using TrackingApi.Contracts.Requests;
using TrackingApi.Features.Legacy;
using TrackingApi.Infrastructure.Auth;
using TrackingApi.Infrastructure.Requests;
using TrackingApi.Infrastructure.Responses;

namespace TrackingApi.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v1")]
public sealed class TrackingLegacyController : ControllerBase
{
    private static readonly string[] LoginKeys = ["email", "password"];
    private static readonly string[] CodigoKeys = ["codigo"];
    private static readonly string[] CreateTrackingKeys =
    [
        "id_traking",
        "cod_pedido",
        "id_delivery",
        "nombreArchivo",
        "codigoZebra",
        "nombres",
        "apellidos",
        "commune_code"
    ];

    private readonly IOptions<AuthOptions> _authOptions;
    private readonly JwtTokenService _jwtTokenService;
    private readonly LegacyRequestReader _requestReader;
    private readonly LegacyTrackingService _trackingService;

    public TrackingLegacyController(
        IOptions<AuthOptions> authOptions,
        JwtTokenService jwtTokenService,
        LegacyRequestReader requestReader,
        LegacyTrackingService trackingService)
    {
        _authOptions = authOptions;
        _jwtTokenService = jwtTokenService;
        _requestReader = requestReader;
        _trackingService = trackingService;
    }

    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] LegacyLoginRequest request, CancellationToken cancellationToken)
    {
        var values = await _requestReader.ReadValuesAsync(Request, LoginKeys);
        var missingFields = LegacyRequestReader.GetMissingFields(values, LoginKeys);
        if (missingFields.Count > 0)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.MissingFields(missingFields)));
        }

        var configuredAuth = _authOptions.Value;
        var email = values["email"]!;
        var password = values["password"]!;

        var validCredentials =
            string.Equals(email, configuredAuth.Email, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(password, configuredAuth.Password, StringComparison.Ordinal);

        if (!validCredentials)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.InvalidCredentials));
        }

        var displayName = string.IsNullOrWhiteSpace(configuredAuth.DisplayName)
            ? email
            : configuredAuth.DisplayName;

        var tokenResult = _jwtTokenService.CreateToken(email, displayName);

        var response = LegacyResponses.Success(
            LegacyResponseMessages.LoginSuccess,
            new LoginResponseData
            {
                Token = tokenResult.Token,
                Type = "Bearer",
                Name = displayName,
                ExpiredAt = tokenResult.ExpiresAtLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            });

        return Ok(response);
    }

    [Consumes("multipart/form-data")]
    [HttpPost("etiqueta")]
    public async Task<IActionResult> Etiqueta([FromForm] LegacyCodigoRequest request, CancellationToken cancellationToken)
    {
        var values = await _requestReader.ReadValuesAsync(Request, CodigoKeys);
        var missingFields = LegacyRequestReader.GetMissingFields(values, CodigoKeys);
        if (missingFields.Count > 0)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.MissingFields(missingFields)));
        }

        var label = await _trackingService.GetLabelAsync(values["codigo"]!, cancellationToken);
        if (label is null)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.TrackingNotFound));
        }

        var response = LegacyResponses.Success(
            LegacyResponseMessages.TransactionOk,
            new LabelResponseData
            {
                IdDelivery = label.IdDelivery,
                IdPedido = label.IdPedido,
                CodigoZebra = label.CodigoZebra
            });

        return Ok(response);
    }

    [Consumes("multipart/form-data")]
    [HttpPost("estado_pedido")]
    public async Task<IActionResult> EstadoPedido([FromForm] LegacyCodigoRequest request, CancellationToken cancellationToken)
    {
        var values = await _requestReader.ReadValuesAsync(Request, CodigoKeys);
        var missingFields = LegacyRequestReader.GetMissingFields(values, CodigoKeys);
        if (missingFields.Count > 0)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.MissingFields(missingFields)));
        }

        var status = await _trackingService.GetStatusAsync(values["codigo"]!, cancellationToken);
        if (status is null)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.TrackingNotFound));
        }

        var response = LegacyResponses.Success(
            LegacyResponseMessages.TransactionOk,
            new StatusResponseData
            {
                Status = status
            });

        return Ok(response);
    }

    [Consumes("multipart/form-data")]
    [HttpPost("orders_tracking")]
    public async Task<IActionResult> OrdersTracking([FromForm] LegacyOrdersTrackingRequest request, CancellationToken cancellationToken)
    {
        var values = await _requestReader.ReadValuesAsync(Request, CreateTrackingKeys);
        var missingFields = LegacyRequestReader.GetMissingFields(values, CreateTrackingKeys);
        if (missingFields.Count > 0)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.MissingFields(missingFields)));
        }

        if (!Guid.TryParse(values["cod_pedido"], out var codPedido))
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.MissingFields(["cod_pedido"])));
        }

        var createResult = await _trackingService.CreateTrackingAsync(
            codPedido,
            values["id_delivery"]!,
            values["nombreArchivo"]!,
            values["codigoZebra"]!,
            cancellationToken);

        if (!createResult.Created)
        {
            return Ok(LegacyResponses.Error(LegacyResponseMessages.TrackingAlreadyExists));
        }

        var response = LegacyResponses.Success(
            LegacyResponseMessages.TransactionOk,
            new CreateTrackingResponseData
            {
                Status = createResult.Status
            });

        return Ok(response);
    }
}
