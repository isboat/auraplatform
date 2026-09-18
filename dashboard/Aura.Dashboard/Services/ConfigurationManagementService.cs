using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class ConfigurationManagementService(
    IConfigurationRepository configurationRepository,
    IAuditRepository auditRepository) : IConfigurationManagementService
{
    public Task<PlatformConfiguration> GetAsync() => configurationRepository.GetAsync();

    public async Task UpdateAsync(bool registrationEnabled, bool uploadsEnabled, StaffActor actor)
    {
        var current = await configurationRepository.GetAsync();
        var previousState = Describe(current);

        current.RegistrationEnabled = registrationEnabled;
        current.UploadsEnabled = uploadsEnabled;

        await configurationRepository.SaveAsync(current);
        await auditRepository.AppendAsync(new AuditEvent
        {
            EventType = "PlatformConfigurationUpdated",
            ActorId = actor.Id,
            ActorName = actor.Name,
            ActorEmail = actor.Email,
            ActorRole = actor.Role,
            TargetType = "PlatformConfiguration",
            TargetId = current.Id,
            TargetDisplay = "Registration and uploads",
            PreviousStatus = previousState,
            NewStatus = Describe(current),
            CorrelationId = actor.CorrelationId
        });
    }

    private static string Describe(PlatformConfiguration configuration) =>
        $"Registration: {(configuration.RegistrationEnabled ? "Enabled" : "Disabled")}; " +
        $"Uploads: {(configuration.UploadsEnabled ? "Enabled" : "Disabled")}";
}
