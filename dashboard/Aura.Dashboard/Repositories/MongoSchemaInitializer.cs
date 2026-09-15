using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoSchemaInitializer(MongoContext context, ILogger<MongoSchemaInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            new CreateIndexModel<StaffUser>(
                Builders<StaffUser>.IndexKeys.Ascending(user => user.Email),
                new CreateIndexOptions { Unique = true, Name = "ux_staffuser_email" }),
            new CreateIndexModel<StaffUser>(
                Builders<StaffUser>.IndexKeys.Ascending(user => user.IsBlocked).Ascending(user => user.Roles),
                new CreateIndexOptions { Name = "ix_staffuser_state_roles" })
        };

        await context.StaffUsers.Indexes.CreateManyAsync(indexes, cancellationToken);
        logger.LogInformation(
            "Ensured MongoDB collection {CollectionName} and its management identity indexes.",
            MongoCollectionNames.StaffUsers);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
