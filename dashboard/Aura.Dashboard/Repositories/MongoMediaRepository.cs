using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

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
