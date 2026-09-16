using Aura.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aura.Api.Services;

public interface IMongoHealthProbe
{
    Task PingAsync(CancellationToken cancellationToken);
}

public sealed class MongoContext : IMongoHealthProbe
{
    public MongoContext(IConfiguration configuration)
    {
        var settings = MongoClientSettings.FromConnectionString(configuration["MongoDb:ConnectionString"] ?? "mongodb://localhost:27017");
        // Set the ServerApi field of the settings object to set the version of the Stable API on the client
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        _database = new MongoClient(settings).GetDatabase(configuration["MongoDb:DatabaseName"] ?? "aura");
    }
    private readonly IMongoDatabase _database;

    public IMongoCollection<UserDocument> Users => _database.GetCollection<UserDocument>("users");
    public IMongoCollection<MediaDocument> Media => _database.GetCollection<MediaDocument>("media");
    public IMongoCollection<CommentDocument> Comments => _database.GetCollection<CommentDocument>("comments");
    public IMongoCollection<PlatformConfiguration> Configuration => _database.GetCollection<PlatformConfiguration>("configuration");

    public Task PingAsync(CancellationToken cancellationToken) =>
        _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
}
