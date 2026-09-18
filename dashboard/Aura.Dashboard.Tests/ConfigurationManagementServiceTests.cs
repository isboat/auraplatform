using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Moq;
using Xunit;

namespace Aura.Dashboard.Tests;

public sealed class ConfigurationManagementServiceTests
{
    [Fact]
    public async Task Update_persists_both_settings_and_records_the_change()
    {
        var configuration = new PlatformConfiguration
        {
            RegistrationEnabled = true,
            UploadsEnabled = true
        };
        var repository = new Mock<IConfigurationRepository>();
        repository.Setup(x => x.GetAsync()).ReturnsAsync(configuration);
        var audit = new Mock<IAuditRepository>();
        var actor = new StaffActor("admin-1", "Ada", "ada@example.com", Roles.Administrator, "trace-1");

        var service = new ConfigurationManagementService(repository.Object, audit.Object);
        await service.UpdateAsync(false, true, actor);

        repository.Verify(x => x.SaveAsync(It.Is<PlatformConfiguration>(value =>
            !value.RegistrationEnabled && value.UploadsEnabled)));
        audit.Verify(x => x.AppendAsync(It.Is<AuditEvent>(entry =>
            entry.EventType == "PlatformConfigurationUpdated" &&
            entry.PreviousStatus == "Registration: Enabled; Uploads: Enabled" &&
            entry.NewStatus == "Registration: Disabled; Uploads: Enabled" &&
            entry.ActorId == actor.Id &&
            entry.CorrelationId == actor.CorrelationId)));
    }

    [Fact]
    public async Task Get_returns_the_repository_configuration()
    {
        var expected = new PlatformConfiguration { UploadsEnabled = false };
        var repository = new Mock<IConfigurationRepository>();
        repository.Setup(x => x.GetAsync()).ReturnsAsync(expected);
        var service = new ConfigurationManagementService(repository.Object, Mock.Of<IAuditRepository>());

        var actual = await service.GetAsync();

        Assert.Same(expected, actual);
    }
}
