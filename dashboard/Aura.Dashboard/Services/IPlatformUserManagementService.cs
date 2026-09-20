using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Services;

public interface IPlatformUserManagementService
{
    Task SetBlockedAsync(string id, bool blocked, string reason, StaffActor actor);
}
