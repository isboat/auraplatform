using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Services;

public interface IConfigurationManagementService
{
    Task<PlatformConfiguration> GetAsync();

    Task UpdateAsync(bool registrationEnabled, bool uploadsEnabled, StaffActor actor);
}
