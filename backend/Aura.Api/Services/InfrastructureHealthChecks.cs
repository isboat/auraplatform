using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aura.Api.Services;

public sealed class MongoDbHealthCheck(IMongoHealthProbe mongo) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await mongo.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy("MongoDB is reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MongoDB is unavailable.", exception);
        }
    }
}

public sealed class MediaStorageHealthCheck(IMediaStorage storage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await storage.CheckHealthAsync(cancellationToken);
            return HealthCheckResult.Healthy("Media storage is reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Media storage is unavailable.", exception);
        }
    }
}
