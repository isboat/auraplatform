using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Moq;
namespace Aura.Api.Tests.Services;

public sealed class ConfigurationServiceTests
{
    [Fact] public async Task Get_delegates_to_repository() { var repo = new Mock<IConfigurationRepository>(); var expected = new PlatformConfiguration(); repo.Setup(x => x.GetAsync()).ReturnsAsync(expected); Assert.Same(expected, await new ConfigurationService(repo.Object).GetAsync()); }
    [Fact] public async Task Update_persists_both_flags() { var repo = new Mock<IConfigurationRepository>(); PlatformConfiguration? saved = null; repo.Setup(x => x.SaveAsync(It.IsAny<PlatformConfiguration>())).Callback<PlatformConfiguration>(x => saved = x).Returns(Task.CompletedTask); var result = await new ConfigurationService(repo.Object).UpdateAsync(new(false, true)); Assert.Same(saved, result); Assert.False(result.RegistrationEnabled); Assert.True(result.UploadsEnabled); }
}
