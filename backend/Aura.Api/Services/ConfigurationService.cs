using Aura.Api.Models;
using Aura.Api.Repositories;

namespace Aura.Api.Services;

public sealed class ConfigurationService(IConfigurationRepository repository) : IConfigurationService
{
    public Task<PlatformConfiguration> GetAsync() => repository.GetAsync();
    public async Task<PlatformConfiguration> UpdateAsync(ConfigurationRequest request)
    {
        var value = new PlatformConfiguration { RegistrationEnabled = request.RegistrationEnabled, UploadsEnabled = request.UploadsEnabled };
        await repository.SaveAsync(value);
        return value;
    }
}
