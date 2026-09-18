using Aura.Api.Models;
using Aura.Api.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Aura.Api.Services;

public sealed class ConfigurationService(IConfigurationRepository repository, IMemoryCache cache) : IConfigurationService
{
    private const string CacheKey = "platform-configuration";
    private static readonly SemaphoreSlim CacheGate = new(1, 1);

    public async Task<PlatformConfiguration> GetAsync()
    {
        if (cache.TryGetValue<PlatformConfiguration>(CacheKey, out var cached))
            return cached!;

        await CacheGate.WaitAsync();
        try
        {
            if (cache.TryGetValue<PlatformConfiguration>(CacheKey, out cached))
                return cached!;

            var value = await repository.GetAsync();
            cache.Set(CacheKey, value, TimeSpan.FromMinutes(1));
            return value;
        }
        finally
        {
            CacheGate.Release();
        }
    }

    public async Task<PlatformConfiguration> UpdateAsync(ConfigurationRequest request)
    {
        var value = new PlatformConfiguration { RegistrationEnabled = request.RegistrationEnabled, UploadsEnabled = request.UploadsEnabled };
        await CacheGate.WaitAsync();
        try
        {
            await repository.SaveAsync(value);
            cache.Set(CacheKey, value, TimeSpan.FromMinutes(1));
            return value;
        }
        finally
        {
            CacheGate.Release();
        }
    }
}
