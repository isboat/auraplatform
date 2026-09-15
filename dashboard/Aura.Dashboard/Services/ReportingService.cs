using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class ReportingService(IMediaRepository media,IUserRepository users):IReportingService
{public async Task<DashboardMetrics> GetAsync(){var m=await media.CountsAsync();var u=await users.CountsAsync();long G(Dictionary<string,long>d,string k)=>d.GetValueOrDefault(k);return new(G(m,"Total")>0?G(m,"Total"):G(m,"video")+G(m,"image")+G(m,"audio"),G(m,"video"),G(m,"image"),G(m,"audio"),G(m,ReviewStates.InReview),G(m,ReviewStates.Approved),G(m,ReviewStates.Rejected),G(u,"Total"),G(u,"Verified"),G(u,"Blocked"),G(u,"Reviewers"),G(u,"Administrators"));}}
