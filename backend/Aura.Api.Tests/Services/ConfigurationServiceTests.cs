using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Moq;
namespace Aura.Api.Tests.Services;

public sealed class ConfigurationServiceTests
{
    [Fact]
    public async Task Get_reads_current_repository_result_every_time()
    {
        var repo = new Mock<IConfigurationRepository>();
        var original = new PlatformConfiguration { RegistrationEnabled = true };
        var updated = new PlatformConfiguration { RegistrationEnabled = false };
        repo.SetupSequence(x => x.GetAsync()).ReturnsAsync(original).ReturnsAsync(updated);
        var service = new ConfigurationService(repo.Object);

        Assert.Same(original, await service.GetAsync());
        Assert.Same(updated, await service.GetAsync());

        repo.Verify(x => x.GetAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Update_persists_and_returns_configuration()
    {
        var repo = new Mock<IConfigurationRepository>();
        PlatformConfiguration? saved = null;
        repo.Setup(x => x.SaveAsync(It.IsAny<PlatformConfiguration>())).Callback<PlatformConfiguration>(x => saved = x).Returns(Task.CompletedTask);
        var service = new ConfigurationService(repo.Object);

        var result = await service.UpdateAsync(new(false, true));

        Assert.Same(saved, result);
        Assert.False(result.RegistrationEnabled);
        Assert.True(result.UploadsEnabled);
        repo.Verify(x => x.SaveAsync(result), Times.Once);
    }
}
