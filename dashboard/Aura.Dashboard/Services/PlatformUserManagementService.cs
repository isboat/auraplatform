using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class PlatformUserManagementService(
    IPlatformUserRepository users,
    IAuditRepository audit) : IPlatformUserManagementService
{
    public async Task SetBlockedAsync(string id, bool blocked, string reason, StaffActor actor)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DashboardRuleException("A reason is required.");

        var user = await users.FindAsync(id)
            ?? throw new DashboardRuleException("Platform user was not found.");
        if (user.IsBlocked == blocked)
            throw new DashboardRuleException($"This platform user is already {(blocked ? "blocked" : "active")}.");
        if (!await users.SetBlockedAsync(id, blocked))
            throw new DashboardRuleException("Platform user was not found.");

        await audit.AppendAsync(ModerationService.Event(
            blocked ? "PlatformUserBlocked" : "PlatformUserUnblocked",
            actor,
            "PlatformUser",
            id,
            user.Email,
            blocked ? "Active" : "Blocked",
            blocked ? "Blocked" : "Active",
            reason.Trim()));
    }
}
