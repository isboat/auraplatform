using Aura.Api.Models;
using MongoDB.Driver;

namespace Aura.Api.Services;

public sealed class MongoContext(IConfiguration configuration)
{
    private readonly IMongoDatabase _database = new MongoClient(configuration["MongoDb:ConnectionString"])
        .GetDatabase(configuration["MongoDb:DatabaseName"] ?? "aura");

    public IMongoCollection<UserDocument> Users => _database.GetCollection<UserDocument>("users");
    public IMongoCollection<MediaDocument> Media => _database.GetCollection<MediaDocument>("media");
    public IMongoCollection<CommentDocument> Comments => _database.GetCollection<CommentDocument>("comments");
    public IMongoCollection<PlatformConfiguration> Configuration => _database.GetCollection<PlatformConfiguration>("configuration");
}
