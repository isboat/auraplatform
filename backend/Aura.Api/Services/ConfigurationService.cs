using Aura.Api.Models;
using Aura.Api.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Aura.Api.Services;

public sealed class ConfigurationService(IConfigurationRepository repository, IMemoryCache cache) : IConfigurationService
{
    private const string CacheKey = "platform-configuration";

    public Task<PlatformConfiguration> GetAsync() => cache.GetOrCreateAsync(
        CacheKey,
        entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return repository.GetAsync();
        })!;

    public async Task<PlatformConfiguration> UpdateAsync(ConfigurationRequest request)
    {
        var value = new PlatformConfiguration { RegistrationEnabled = request.RegistrationEnabled, UploadsEnabled = request.UploadsEnabled };
        await repository.SaveAsync(value);
        cache.Set(CacheKey, value, TimeSpan.FromMinutes(1));
        return value;
    }
}
