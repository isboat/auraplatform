using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoContext(IConfiguration configuration)
{
    public static class CollectionNames
    {
        // Dashboard identities deliberately live outside the public application's
        // users collection so management access has an isolated security boundary.
        public const string StaffUsers = "staffuser";
        public const string BootstrapLocks = "managementBootstrapLocks";
        public const string Media = "media";
        public const string Audit = "managementAudit";
    }

    private readonly IMongoDatabase _db = new MongoClient(configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017").GetDatabase(configuration["Mongo:Database"] ?? "aura");
    public IMongoCollection<StaffUser> StaffUsers => _db.GetCollection<StaffUser>(CollectionNames.StaffUsers);
    public IMongoCollection<MongoDB.Bson.BsonDocument> BootstrapLocks => _db.GetCollection<MongoDB.Bson.BsonDocument>(CollectionNames.BootstrapLocks);
    public IMongoCollection<ManagedMedia> Media => _db.GetCollection<ManagedMedia>(CollectionNames.Media);
    public IMongoCollection<AuditEvent> Audit => _db.GetCollection<AuditEvent>(CollectionNames.Audit);
}

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
            MongoContext.CollectionNames.StaffUsers);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class MongoMediaRepository(MongoContext context) : IMediaRepository
{
    public async Task<PageResult<ManagedMedia>> SearchAsync(string? query, string? type, string? status, DateTime? from, DateTime? to, int page, int size)
    {
        var f = Builders<ManagedMedia>.Filter; var filters = new List<FilterDefinition<ManagedMedia>>();
        if (!string.IsNullOrWhiteSpace(query)) filters.Add(f.Or(f.Regex(x=>x.Title, new(query,"i")), f.Regex(x=>x.OwnerName,new(query,"i")), f.AnyEq(x=>x.Tags,query)));
        if (!string.IsNullOrWhiteSpace(type)) filters.Add(f.Eq(x=>x.MediaType,type)); if (!string.IsNullOrWhiteSpace(status)) filters.Add(f.Eq(x=>x.ReviewStatus,status));
        if (from.HasValue) filters.Add(f.Gte(x=>x.CreatedAt,from.Value)); if (to.HasValue) filters.Add(f.Lt(x=>x.CreatedAt,to.Value.AddDays(1)));
        var filter=filters.Count==0?f.Empty:f.And(filters); var total=await context.Media.CountDocumentsAsync(filter);
        var items=await context.Media.Find(filter).SortByDescending(x=>x.CreatedAt).Skip((page-1)*size).Limit(size).ToListAsync(); return new(items,total,page,size);
    }
    public Task<ManagedMedia?> FindAsync(string id)=>context.Media.Find(x=>x.Id==id).FirstOrDefaultAsync();
    public async Task<bool> TrySetReviewStatusAsync(string id,string expected,string next)=>(await context.Media.UpdateOneAsync(x=>x.Id==id&&x.ReviewStatus==expected,Builders<ManagedMedia>.Update.Set(x=>x.ReviewStatus,next))).ModifiedCount==1;
    public Task DeleteAsync(string id)=>context.Media.DeleteOneAsync(x=>x.Id==id);
    public async Task<Dictionary<string,long>> CountsAsync(){var all=await context.Media.Find(_=>true).ToListAsync(); return all.GroupBy(x=>x.ReviewStatus).Concat(all.GroupBy(x=>x.MediaType)).ToDictionary(x=>x.Key,x=>(long)x.Count(),StringComparer.OrdinalIgnoreCase);}
}

public sealed class MongoUserRepository(MongoContext context) : IUserRepository
{
    public Task<StaffUser?> FindAsync(string id)=>context.StaffUsers.Find(x=>x.Id==id).FirstOrDefaultAsync(); public Task<StaffUser?> FindByEmailAsync(string email)=>context.StaffUsers.Find(x=>x.Email==email.ToLowerInvariant()).FirstOrDefaultAsync();
    public async Task<bool> HasAdministratorAsync()
    {
        var filter = Builders<StaffUser>.Filter.Or(
            Builders<StaffUser>.Filter.Eq(user => user.IsAdministrator, true),
            Builders<StaffUser>.Filter.AnyEq(user => user.Roles, Roles.Administrator));
        return await context.StaffUsers.CountDocumentsAsync(
            filter,
            new CountOptions { Limit = 1 }) > 0;
    }
    public async Task<bool> TryAcquireFirstAdministratorBootstrapAsync()
    {
        try
        {
            await context.BootstrapLocks.InsertOneAsync(new MongoDB.Bson.BsonDocument
            {
                ["_id"] = "first-administrator",
                ["createdAtUtc"] = DateTime.UtcNow
            });
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
    public Task ReleaseFirstAdministratorBootstrapAsync() => context.BootstrapLocks
        .DeleteOneAsync(Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", "first-administrator"));
    public async Task<PageResult<StaffUser>> SearchAsync(string? q,string? role,bool? blocked,int page,int size){var f=Builders<StaffUser>.Filter;var fs=new List<FilterDefinition<StaffUser>>();if(!string.IsNullOrWhiteSpace(q))fs.Add(f.Or(f.Regex(x=>x.Name,new(q,"i")),f.Regex(x=>x.Email,new(q,"i"))));if(!string.IsNullOrWhiteSpace(role))fs.Add(role==Roles.Administrator?f.Or(f.Eq(x=>x.IsAdministrator,true),f.AnyEq(x=>x.Roles,role)):f.AnyEq(x=>x.Roles,role));if(blocked.HasValue)fs.Add(f.Eq(x=>x.IsBlocked,blocked));var filter=fs.Count==0?f.Empty:f.And(fs);var total=await context.StaffUsers.CountDocumentsAsync(filter);var items=await context.StaffUsers.Find(filter).SortBy(x=>x.Name).Skip((page-1)*size).Limit(size).ToListAsync();return new(items,total,page,size);}
    public async Task SaveAsync(StaffUser u){if(u.Id is null)await context.StaffUsers.InsertOneAsync(u);else await context.StaffUsers.ReplaceOneAsync(x=>x.Id==u.Id,u,new ReplaceOptions{IsUpsert=false});} public Task DeleteAsync(string id)=>context.StaffUsers.DeleteOneAsync(x=>x.Id==id);
    public async Task<Dictionary<string,long>> CountsAsync(){var all=await context.StaffUsers.Find(_=>true).ToListAsync();return new(){{"Total",all.Count},{"Verified",all.Count(x=>x.EmailVerified)},{"Blocked",all.Count(x=>x.IsBlocked)},{"Reviewers",all.Count(x=>x.IsReviewer&&!x.IsAdmin)},{"Administrators",all.Count(x=>x.IsAdmin)}};}
}
public sealed class MongoAuditRepository(MongoContext context):IAuditRepository
{
    public Task AppendAsync(AuditEvent e)=>context.Audit.InsertOneAsync(e);
    public async Task<PageResult<AuditEvent>> SearchAsync(string? type,string? actor,string? target,string? outcome,DateTime? from,DateTime? to,int page,int size){var f=Builders<AuditEvent>.Filter;var fs=new List<FilterDefinition<AuditEvent>>();if(!string.IsNullOrWhiteSpace(type))fs.Add(f.Eq(x=>x.EventType,type));if(!string.IsNullOrWhiteSpace(actor))fs.Add(f.Regex(x=>x.ActorEmail,new(actor,"i")));if(!string.IsNullOrWhiteSpace(target))fs.Add(f.Regex(x=>x.TargetDisplay,new(target,"i")));if(!string.IsNullOrWhiteSpace(outcome))fs.Add(f.Eq(x=>x.Outcome,outcome));if(from.HasValue)fs.Add(f.Gte(x=>x.OccurredAtUtc,from));if(to.HasValue)fs.Add(f.Lt(x=>x.OccurredAtUtc,to.Value.AddDays(1)));var filter=fs.Count==0?f.Empty:f.And(fs);var total=await context.Audit.CountDocumentsAsync(filter);var items=await context.Audit.Find(filter).SortByDescending(x=>x.OccurredAtUtc).Skip((page-1)*size).Limit(size).ToListAsync();return new(items,total,page,size);}
}
