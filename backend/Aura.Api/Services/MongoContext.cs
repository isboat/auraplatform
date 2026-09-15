using Aura.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aura.Api.Services;

public interface IMongoHealthProbe
{
    Task PingAsync(CancellationToken cancellationToken);
}

public sealed class MongoContext(IConfiguration configuration) : IMongoHealthProbe
{
    private readonly IMongoDatabase _database = new MongoClient(configuration["MongoDb:ConnectionString"])
        .GetDatabase(configuration["MongoDb:DatabaseName"] ?? "aura");

    public IMongoCollection<UserDocument> Users => _database.GetCollection<UserDocument>("users");
    public IMongoCollection<MediaDocument> Media => _database.GetCollection<MediaDocument>("media");
    public IMongoCollection<CommentDocument> Comments => _database.GetCollection<CommentDocument>("comments");
    public IMongoCollection<PlatformConfiguration> Configuration => _database.GetCollection<PlatformConfiguration>("configuration");

    public Task PingAsync(CancellationToken cancellationToken) =>
        _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
}
