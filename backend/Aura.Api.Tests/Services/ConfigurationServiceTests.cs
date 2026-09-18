using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
namespace Aura.Api.Tests.Services;

public sealed class ConfigurationServiceTests
{
    [Fact]
    public async Task Get_caches_repository_result_for_one_minute()
    {
        var repo = new Mock<IConfigurationRepository>();
        var expected = new PlatformConfiguration { RegistrationEnabled = true };
        repo.Setup(x => x.GetAsync()).ReturnsAsync(expected);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ConfigurationService(repo.Object, cache);

        Assert.Equal(TimeSpan.FromMinutes(1), ConfigurationService.CacheDuration);
        Assert.Same(expected, await service.GetAsync());
        Assert.Same(expected, await service.GetAsync());

        repo.Verify(x => x.GetAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_persists_and_returns_configuration()
    {
        var repo = new Mock<IConfigurationRepository>();
        PlatformConfiguration? saved = null;
        repo.Setup(x => x.SaveAsync(It.IsAny<PlatformConfiguration>())).Callback<PlatformConfiguration>(x => saved = x).Returns(Task.CompletedTask);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ConfigurationService(repo.Object, cache);

        var result = await service.UpdateAsync(new(false, true));

        Assert.Same(saved, result);
        Assert.False(result.RegistrationEnabled);
        Assert.True(result.UploadsEnabled);
        Assert.Same(result, await service.GetAsync());
        repo.Verify(x => x.SaveAsync(result), Times.Once);
        repo.Verify(x => x.GetAsync(), Times.Never);
    }
}
