using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Aura.Dashboard.Services;

public sealed class ReportingService(IMediaRepository media,IUserRepository users,IMemoryCache cache):IReportingService
{
 private const string CacheKey="dashboard-metrics";
 public Task<DashboardMetrics> GetAsync()=>cache.GetOrCreateAsync<DashboardMetrics>(CacheKey,async entry=>
 {
  // Dashboard writes may originate in another process, so keep this local cache deliberately short-lived.
  entry.AbsoluteExpirationRelativeToNow=TimeSpan.FromSeconds(15);
  var m=await media.CountsAsync();var u=await users.CountsAsync();long G(Dictionary<string,long>d,string k)=>d.GetValueOrDefault(k);return new(G(m,"Total")>0?G(m,"Total"):G(m,"video")+G(m,"image")+G(m,"audio"),G(m,"video"),G(m,"image"),G(m,"audio"),G(m,ReviewStates.InReview),G(m,ReviewStates.Approved),G(m,ReviewStates.Rejected),G(u,"Total"),G(u,"Verified"),G(u,"Blocked"),G(u,"Reviewers"),G(u,"Administrators"));
 })!;
}
