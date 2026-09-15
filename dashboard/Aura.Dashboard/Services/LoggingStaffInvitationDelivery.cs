using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class LoggingStaffInvitationDelivery(ILogger<LoggingStaffInvitationDelivery> logger) : IStaffInvitationDelivery
{
    public Task SendAsync(StaffUser user, string token, DateTime expiresAtUtc)
    {
        logger.LogInformation("Staff invitation delivery requested for user {UserId}; expires {ExpiresAtUtc}", user.Id, expiresAtUtc);
        return Task.CompletedTask;
    }
}
