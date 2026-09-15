using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;
public interface IModerationService { Task DecideAsync(string mediaId,bool approve,string? reason,StaffActor actor); }
public interface IUserManagementService { Task SetBlockedAsync(string id,bool blocked,string reason,StaffActor actor); Task SetReviewerAsync(string id,bool enabled,StaffActor actor); Task PromoteAdministratorAsync(string id,StaffActor actor); Task RequestResetAsync(string id,StaffActor actor); Task DeleteAsync(string id,string reason,StaffActor actor); }
public interface IMediaManagementService { Task DeleteAsync(string id,string reason,StaffActor actor); }
public interface IReportingService { Task<DashboardMetrics> GetAsync(); }

public sealed class ModerationService(IMediaRepository media,IAuditRepository audit):IModerationService
{
    public async Task DecideAsync(string id,bool approve,string? reason,StaffActor actor)
    {
        if(!approve&&string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A rejection reason is required.");
        var item=await media.FindAsync(id)??throw new DashboardRuleException("Media was not found."); var next=approve?ReviewStates.Approved:ReviewStates.Rejected;
        if(!await media.TrySetReviewStatusAsync(id,ReviewStates.InReview,next))throw new ConcurrencyException("This upload has already been reviewed.");
        await audit.AppendAsync(Event(approve?"MediaApproved":"MediaRejected",actor,"Media",id,item.Title,ReviewStates.InReview,next,reason));
    }
    internal static AuditEvent Event(string type,StaffActor a,string targetType,string id,string display,string? old=null,string? next=null,string? reason=null)=>new(){EventType=type,ActorId=a.Id,ActorName=a.Name,ActorEmail=a.Email,ActorRole=a.Role,TargetType=targetType,TargetId=id,TargetDisplay=display,PreviousStatus=old,NewStatus=next,Reason=reason,CorrelationId=a.CorrelationId};
}
public sealed class UserManagementService(IUserRepository users,IAuditRepository audit,IResetDelivery reset):IUserManagementService
{
    private async Task<StaffUser> Manageable(string id){var u=await users.FindAsync(id)??throw new DashboardRuleException("User was not found.");if(u.IsAdmin)throw new DashboardRuleException("Administrator accounts cannot be managed in the dashboard.");return u;}
    public async Task SetBlockedAsync(string id,bool blocked,string reason,StaffActor actor){if(string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A reason is required.");var u=await Manageable(id);u.IsBlocked=blocked;u.SessionVersion++;await users.SaveAsync(u);await audit.AppendAsync(ModerationService.Event(blocked?"UserBlocked":"UserUnblocked",actor,"User",id,u.Email,!blocked?"Blocked":"Active",blocked?"Blocked":"Active",reason));}
    public async Task SetReviewerAsync(string id,bool enabled,StaffActor actor){var u=await Manageable(id);if(enabled&&(!u.EmailVerified||u.IsBlocked))throw new DashboardRuleException("Reviewer access requires an active, verified account.");if(enabled){if(!u.Roles.Contains(Roles.Reviewer))u.Roles.Add(Roles.Reviewer);}else{u.Roles.Remove(Roles.Reviewer);u.SessionVersion++;}await users.SaveAsync(u);await audit.AppendAsync(ModerationService.Event(enabled?"ReviewerAdded":"ReviewerRemoved",actor,"User",id,u.Email));}
    public async Task PromoteAdministratorAsync(string id,StaffActor actor){var u=await Manageable(id);if(!u.EmailVerified||u.IsBlocked)throw new DashboardRuleException("Administrator access requires an active, verified account.");u.IsAdministrator=true;if(!u.Roles.Contains(Roles.Administrator))u.Roles.Add(Roles.Administrator);await users.SaveAsync(u);await audit.AppendAsync(ModerationService.Event("AdministratorAdded",actor,"User",id,u.Email));}
    public async Task RequestResetAsync(string id,StaffActor actor){var u=await Manageable(id);await reset.SendAsync(u);await audit.AppendAsync(ModerationService.Event("PasswordResetRequested",actor,"User",id,u.Email));}
    public async Task DeleteAsync(string id,string reason,StaffActor actor){if(string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A deletion reason is required.");var u=await Manageable(id);await users.DeleteAsync(id);await audit.AppendAsync(ModerationService.Event("UserDeleted",actor,"User",id,u.Email,reason:reason));}
}
public sealed class MediaManagementService(IMediaRepository media,IAssetStorage storage,IAuditRepository audit):IMediaManagementService
{public async Task DeleteAsync(string id,string reason,StaffActor actor){if(string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A deletion reason is required.");var item=await media.FindAsync(id)??throw new DashboardRuleException("Media was not found.");await storage.DeleteAsync(item.ObjectKey);await media.DeleteAsync(id);await audit.AppendAsync(ModerationService.Event("MediaDeleted",actor,"Media",id,item.Title,reason:reason));}}
public sealed class ReportingService(IMediaRepository media,IUserRepository users):IReportingService
{public async Task<DashboardMetrics> GetAsync(){var m=await media.CountsAsync();var u=await users.CountsAsync();long G(Dictionary<string,long>d,string k)=>d.GetValueOrDefault(k);return new(G(m,"Total")>0?G(m,"Total"):G(m,"video")+G(m,"image")+G(m,"audio"),G(m,"video"),G(m,"image"),G(m,"audio"),G(m,ReviewStates.InReview),G(m,ReviewStates.Approved),G(m,ReviewStates.Rejected),G(u,"Total"),G(u,"Verified"),G(u,"Blocked"),G(u,"Reviewers"),G(u,"Administrators"));}}

public sealed class ConfiguredAssetStorage(IConfiguration config):IAssetStorage { public Task DeleteAsync(string key)=>Task.CompletedTask; public string ReadUrl(string key)=>$"{config["MediaStorage:PublicBaseUrl"]?.TrimEnd('/')}/{Uri.EscapeDataString(key)}"; }
public sealed class LoggingResetDelivery(ILogger<LoggingResetDelivery> logger):IResetDelivery { public Task SendAsync(StaffUser user){logger.LogInformation("Password reset requested for user {UserId}",user.Id);return Task.CompletedTask;} }
