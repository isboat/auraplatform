using Aura.Api.Models;
using Aura.Api.Services;
using MongoDB.Driver;

namespace Aura.Api.Repositories;

public sealed class ConfigurationRepository(MongoContext db) : IConfigurationRepository
{
    public async Task<PlatformConfiguration> GetAsync() => await db.Configuration.Find(x => x.Id == "platform").FirstOrDefaultAsync() ?? new();
    public Task SaveAsync(PlatformConfiguration configuration) => db.Configuration.ReplaceOneAsync(x => x.Id == configuration.Id, configuration, new ReplaceOptions { IsUpsert = true });
}
