using Amazon.S3;
using Amazon.S3.Model;
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

public sealed class S3HealthCheck(IAmazonS3 s3, IConfiguration configuration) : IHealthCheck
{
    private readonly string _bucketName = configuration["AWS:BucketName"]
        ?? throw new InvalidOperationException("AWS:BucketName must be configured.");

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await s3.GetBucketLocationAsync(
                new GetBucketLocationRequest { BucketName = _bucketName },
                cancellationToken);
            return HealthCheckResult.Healthy("Amazon S3 is reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Amazon S3 is unavailable.", exception);
        }
    }
}
