using Amazon.S3;
using Amazon.S3.Model;
using Aura.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace Aura.Api.Tests.Services;

public sealed class InfrastructureHealthChecksTests
{
    [Fact]
    public async Task MongoDb_ReturnsHealthy_WhenPingSucceeds()
    {
        var mongo = new Mock<IMongoHealthProbe>();
        var result = await new MongoDbHealthCheck(mongo.Object)
            .CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        mongo.Verify(value => value.PingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoDb_ReturnsUnhealthy_WhenPingFails()
    {
        var mongo = new Mock<IMongoHealthProbe>();
        mongo.Setup(value => value.PingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("offline"));

        var result = await new MongoDbHealthCheck(mongo.Object)
            .CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.IsType<InvalidOperationException>(result.Exception);
    }

    [Fact]
    public async Task S3_ReturnsHealthy_WhenBucketIsReachable()
    {
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(value => value.GetBucketLocationAsync(
                It.Is<GetBucketLocationRequest>(request => request.BucketName == "media"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBucketLocationResponse());

        var result = await new S3HealthCheck(s3.Object, Configuration())
            .CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task S3_ReturnsUnhealthy_WhenBucketRequestFails()
    {
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(value => value.GetBucketLocationAsync(
                It.IsAny<GetBucketLocationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("offline"));

        var result = await new S3HealthCheck(s3.Object, Configuration())
            .CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.IsType<AmazonS3Exception>(result.Exception);
    }

    private static IConfiguration Configuration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["AWS:BucketName"] = "media" })
        .Build();
}
