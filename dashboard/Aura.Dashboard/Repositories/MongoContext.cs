using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoContext
{
    private readonly IMongoDatabase _db;

    public MongoContext(IConfiguration configuration)
    {
        var settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017");
        // Set the ServerApi field of the settings object to set the version of the Stable API on the client
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        _db = new MongoClient(settings).GetDatabase(configuration["Mongo:Database"] ?? "aura");
    }

    public IMongoCollection<StaffUser> StaffUsers => _db.GetCollection<StaffUser>(MongoCollectionNames.StaffUsers);
    public IMongoCollection<MongoDB.Bson.BsonDocument> BootstrapLocks => _db.GetCollection<MongoDB.Bson.BsonDocument>(MongoCollectionNames.BootstrapLocks);
    public IMongoCollection<ManagedMedia> Media => _db.GetCollection<ManagedMedia>(MongoCollectionNames.Media);
    public IMongoCollection<AuditEvent> Audit => _db.GetCollection<AuditEvent>(MongoCollectionNames.Audit);
}
