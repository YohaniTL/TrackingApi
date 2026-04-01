using Microsoft.EntityFrameworkCore;
using TrackingApi.Domain.Entities;
using TrackingApi.Infrastructure.Data;

namespace TrackingApi.Features.Legacy;

public sealed class LegacyTrackingService
{
    private readonly TrackingDbContext _dbContext;

    public LegacyTrackingService(TrackingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LabelLookupResult?> GetLabelAsync(string code, CancellationToken cancellationToken)
    {
        var tracking = await FindTrackingByCodeAsync(code, cancellationToken);
        if (tracking is null)
        {
            return null;
        }

        return new LabelLookupResult(
            tracking.IdDelivery,
            tracking.NombreArchivo,
            tracking.CodigoZebra);
    }

    public async Task<string?> GetStatusAsync(string code, CancellationToken cancellationToken)
    {
        var tracking = await FindTrackingByCodeAsync(code, cancellationToken);
        if (tracking is null)
        {
            return null;
        }

        var orderShippingStatus = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.CodPedido == tracking.CodPedido)
            .Select(order => order.ShippingStatus)
            .FirstOrDefaultAsync(cancellationToken);

        return await ResolveStatusAsync(orderShippingStatus, tracking.ShippingStatus, cancellationToken);
    }

    public async Task<CreateTrackingResult> CreateTrackingAsync(
        Guid codPedido,
        string idDelivery,
        string nombreArchivo,
        string codigoZebra,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.TrackingOrders
            .AsNoTracking()
            .AnyAsync(tracking => tracking.CodPedido == codPedido, cancellationToken);

        if (exists)
        {
            return CreateTrackingResult.Duplicate();
        }

        var entity = new GesEcoOrdersTrackingEntity
        {
            CodPedido = codPedido,
            IdDelivery = idDelivery,
            Tipo = "JSON",
            NombreArchivo = nombreArchivo,
            CodigoZebra = codigoZebra,
            ShippingStatus = null
        };

        _dbContext.TrackingOrders.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateTrackingResult.Success(string.Empty);
    }

    private async Task<GesEcoOrdersTrackingEntity?> FindTrackingByCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim();

        var byDelivery = await _dbContext.TrackingOrders
            .AsNoTracking()
            .Where(tracking => tracking.IdDelivery == normalizedCode)
            .OrderByDescending(tracking => tracking.IdTracking)
            .FirstOrDefaultAsync(cancellationToken);

        if (byDelivery is not null)
        {
            return byDelivery;
        }

        var byOrderNumber = await _dbContext.TrackingOrders
            .AsNoTracking()
            .Where(tracking => tracking.NombreArchivo == normalizedCode)
            .OrderByDescending(tracking => tracking.IdTracking)
            .FirstOrDefaultAsync(cancellationToken);

        if (byOrderNumber is not null)
        {
            return byOrderNumber;
        }

        if (!Guid.TryParse(normalizedCode, out var codPedido))
        {
            return null;
        }

        return await _dbContext.TrackingOrders
            .AsNoTracking()
            .Where(tracking => tracking.CodPedido == codPedido)
            .OrderByDescending(tracking => tracking.IdTracking)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<string> ResolveStatusAsync(
        string? orderShippingStatus,
        Guid? trackingShippingStatus,
        CancellationToken cancellationToken)
    {
        if (Guid.TryParse(orderShippingStatus, out var orderStatusGuid))
        {
            var description = await FindStatusDescriptionAsync(orderStatusGuid, cancellationToken);
            return string.IsNullOrWhiteSpace(description)
                ? orderStatusGuid.ToString()
                : description;
        }

        if (!string.IsNullOrWhiteSpace(orderShippingStatus))
        {
            return orderShippingStatus;
        }

        if (trackingShippingStatus.HasValue)
        {
            var description = await FindStatusDescriptionAsync(trackingShippingStatus.Value, cancellationToken);
            return string.IsNullOrWhiteSpace(description)
                ? trackingShippingStatus.Value.ToString()
                : description;
        }

        return string.Empty;
    }

    private async Task<string?> FindStatusDescriptionAsync(Guid statusId, CancellationToken cancellationToken)
    {
        return await _dbContext.Parameters
            .AsNoTracking()
            .Where(parameter => parameter.CodParametro == statusId)
            .Select(parameter => parameter.Descripcion)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed record LabelLookupResult(string IdDelivery, string IdPedido, string CodigoZebra);

public sealed record CreateTrackingResult(bool Created, string Status)
{
    public static CreateTrackingResult Duplicate() => new(false, string.Empty);

    public static CreateTrackingResult Success(string status) => new(true, status);
}
