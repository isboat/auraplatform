using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Repositories;

public interface IConfigurationRepository
{
    Task<PlatformConfiguration> GetAsync();

    Task SaveAsync(PlatformConfiguration configuration);
}
