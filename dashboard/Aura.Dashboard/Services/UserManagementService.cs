using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class UserManagementService(IUserRepository users,IAuditRepository audit,IResetDelivery reset):IUserManagementService
{
    private async Task<StaffUser> Manageable(string id){var u=await users.FindAsync(id)??throw new DashboardRuleException("User was not found.");if(u.IsAdmin)throw new DashboardRuleException("Administrator accounts cannot be managed in the dashboard.");return u;}
    public async Task SetBlockedAsync(string id,bool blocked,string reason,StaffActor actor){if(string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A reason is required.");var u=await Manageable(id);u.IsBlocked=blocked;u.SessionVersion++;await users.SaveAsync(u);await audit.AppendAsync(ModerationService.Event(blocked?"UserBlocked":"UserUnblocked",actor,"User",id,u.Email,!blocked?"Blocked":"Active",blocked?"Blocked":"Active",reason));}
    public async Task SetReviewerAsync(string id,bool enabled,StaffActor actor){var u=await Manageable(id);if(enabled&&(!u.EmailVerified||u.IsBlocked))throw new DashboardRuleException("Reviewer access requires an active, verified account.");if(enabled){if(!u.Roles.Contains(Roles.Reviewer))u.Roles.Add(Roles.Reviewer);}else{u.Roles.Remove(Roles.Reviewer);u.SessionVersion++;}await users.SaveAsync(u);await audit.AppendAsync(ModerationService.Event(enabled?"ReviewerAdded":"ReviewerRemoved",actor,"User",id,u.Email));}
    public async Task PromoteAdministratorAsync(string id,StaffActor actor){var u=await Manageable(id);if(!u.EmailVerified||u.IsBlocked)throw new DashboardRuleException("Administrator access requires an active, verified account.");u.IsAdministrator=true;if(!u.Roles.Contains(Roles.Administrator))u.Roles.Add(Roles.Administrator);await users.SaveAsync(u);await audit.AppendAsync(ModerationService.Event("AdministratorAdded",actor,"User",id,u.Email));}
    public async Task RequestResetAsync(string id,StaffActor actor){var u=await Manageable(id);await reset.SendAsync(u);await audit.AppendAsync(ModerationService.Event("PasswordResetRequested",actor,"User",id,u.Email));}
    public async Task DeleteAsync(string id,string reason,StaffActor actor){if(string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A deletion reason is required.");var u=await Manageable(id);await users.DeleteAsync(id);await audit.AppendAsync(ModerationService.Event("UserDeleted",actor,"User",id,u.Email,reason:reason));}
}
