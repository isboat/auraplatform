using Aura.Api.Services;
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
    public async Task MediaStorage_ReturnsHealthy_WhenProviderIsReachable()
    {
        var storage = new Mock<IMediaStorage>();

        var result = await new MediaStorageHealthCheck(storage.Object)
            .CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        storage.Verify(value => value.CheckHealthAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MediaStorage_ReturnsUnhealthy_WhenProviderRequestFails()
    {
        var storage = new Mock<IMediaStorage>();
        storage.Setup(value => value.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("offline"));

        var result = await new MediaStorageHealthCheck(storage.Object)
            .CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.IsType<InvalidOperationException>(result.Exception);
    }
}
