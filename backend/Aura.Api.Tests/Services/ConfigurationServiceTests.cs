using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
namespace Aura.Api.Tests.Services;

public sealed class ConfigurationServiceTests
{
    [Fact]
    public async Task Get_caches_repository_result()
    {
        var repo = new Mock<IConfigurationRepository>();
        var expected = new PlatformConfiguration();
        repo.Setup(x => x.GetAsync()).ReturnsAsync(expected);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ConfigurationService(repo.Object, cache);

        Assert.Same(expected, await service.GetAsync());
        Assert.Same(expected, await service.GetAsync());

        repo.Verify(x => x.GetAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_persists_and_refreshes_cache()
    {
        var repo = new Mock<IConfigurationRepository>();
        PlatformConfiguration? saved = null;
        repo.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration());
        repo.Setup(x => x.SaveAsync(It.IsAny<PlatformConfiguration>())).Callback<PlatformConfiguration>(x => saved = x).Returns(Task.CompletedTask);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ConfigurationService(repo.Object, cache);
        await service.GetAsync();

        var result = await service.UpdateAsync(new(false, true));

        Assert.Same(saved, result);
        Assert.Same(result, await service.GetAsync());
        Assert.False(result.RegistrationEnabled);
        Assert.True(result.UploadsEnabled);
        repo.Verify(x => x.GetAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_wins_when_a_cache_fill_is_already_in_progress()
    {
        var readStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRead = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var repo = new Mock<IConfigurationRepository>();
        repo.Setup(x => x.GetAsync()).Returns(async () =>
        {
            readStarted.SetResult(true);
            await releaseRead.Task;
            return new PlatformConfiguration { RegistrationEnabled = true, UploadsEnabled = false };
        });
        repo.Setup(x => x.SaveAsync(It.IsAny<PlatformConfiguration>())).Returns(Task.CompletedTask);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ConfigurationService(repo.Object, cache);

        var fill = service.GetAsync();
        await readStarted.Task;
        var update = service.UpdateAsync(new(false, true));
        Assert.False(update.IsCompleted);

        releaseRead.SetResult(true);
        await fill;
        var updated = await update;

        Assert.Same(updated, await service.GetAsync());
        Assert.False(updated.RegistrationEnabled);
        Assert.True(updated.UploadsEnabled);
        repo.Verify(x => x.GetAsync(), Times.Once);
    }
}
