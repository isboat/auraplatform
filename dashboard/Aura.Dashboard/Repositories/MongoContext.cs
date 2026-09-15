using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoContext(IConfiguration configuration)
{
    private readonly IMongoDatabase _db = new MongoClient(configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017").GetDatabase(configuration["Mongo:Database"] ?? "aura");
    public IMongoCollection<StaffUser> StaffUsers => _db.GetCollection<StaffUser>(MongoCollectionNames.StaffUsers);
    public IMongoCollection<MongoDB.Bson.BsonDocument> BootstrapLocks => _db.GetCollection<MongoDB.Bson.BsonDocument>(MongoCollectionNames.BootstrapLocks);
    public IMongoCollection<ManagedMedia> Media => _db.GetCollection<ManagedMedia>(MongoCollectionNames.Media);
    public IMongoCollection<AuditEvent> Audit => _db.GetCollection<AuditEvent>(MongoCollectionNames.Audit);
}
