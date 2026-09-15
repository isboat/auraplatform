using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public interface IUserManagementService
{
    Task SetBlockedAsync(string id, bool blocked, string reason, StaffActor actor);
    Task SetReviewerAsync(string id, bool enabled, StaffActor actor);
    Task PromoteAdministratorAsync(string id, StaffActor actor);
    Task RequestResetAsync(string id, StaffActor actor);
    Task DeleteAsync(string id, string reason, StaffActor actor);
}
