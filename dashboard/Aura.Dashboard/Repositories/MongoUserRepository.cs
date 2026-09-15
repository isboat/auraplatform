using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

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
