using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TrackingApi.Infrastructure.Data;

namespace TrackingApi.Infrastructure.Health;

public sealed class SqlServerHealthCheck : IHealthCheck
{
    private readonly TrackingDbContext _dbContext;

    public SqlServerHealthCheck(TrackingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var canConnect = await _dbContext.Database.CanConnectAsync(timeoutCts.Token);
            return canConnect
                ? HealthCheckResult.Healthy("SQL Server reachable.")
                : HealthCheckResult.Unhealthy("SQL Server unavailable.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("SQL Server unavailable.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server unavailable.", ex);
        }
    }
}
