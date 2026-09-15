using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoAuditRepository(MongoContext context):IAuditRepository
{
    public Task AppendAsync(AuditEvent e)=>context.Audit.InsertOneAsync(e);
    public async Task<PageResult<AuditEvent>> SearchAsync(string? type,string? actor,string? target,string? outcome,DateTime? from,DateTime? to,int page,int size){var f=Builders<AuditEvent>.Filter;var fs=new List<FilterDefinition<AuditEvent>>();if(!string.IsNullOrWhiteSpace(type))fs.Add(f.Eq(x=>x.EventType,type));if(!string.IsNullOrWhiteSpace(actor))fs.Add(f.Regex(x=>x.ActorEmail,new(actor,"i")));if(!string.IsNullOrWhiteSpace(target))fs.Add(f.Regex(x=>x.TargetDisplay,new(target,"i")));if(!string.IsNullOrWhiteSpace(outcome))fs.Add(f.Eq(x=>x.Outcome,outcome));if(from.HasValue)fs.Add(f.Gte(x=>x.OccurredAtUtc,from));if(to.HasValue)fs.Add(f.Lt(x=>x.OccurredAtUtc,to.Value.AddDays(1)));var filter=fs.Count==0?f.Empty:f.And(fs);var total=await context.Audit.CountDocumentsAsync(filter);var items=await context.Audit.Find(filter).SortByDescending(x=>x.OccurredAtUtc).Skip((page-1)*size).Limit(size).ToListAsync();return new(items,total,page,size);}
}
