using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoConfigurationRepository(MongoContext context) : IConfigurationRepository
{
    public async Task<PlatformConfiguration> GetAsync() =>
        await context.Configuration.Find(configuration => configuration.Id == "platform").FirstOrDefaultAsync()
        ?? new PlatformConfiguration();

    public Task SaveAsync(PlatformConfiguration configuration) =>
        context.Configuration.ReplaceOneAsync(
            current => current.Id == configuration.Id,
            configuration,
            new ReplaceOptions { IsUpsert = true });
}
