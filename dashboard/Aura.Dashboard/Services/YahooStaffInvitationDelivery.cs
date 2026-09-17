using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class YahooStaffInvitationDelivery(IEmailSender sender, DashboardLinkBuilder links) : IStaffInvitationDelivery
{
    public Task SendAsync(StaffUser user, string token, DateTime expiresAtUtc) => sender.SendAsync(
        user.Email,
        "You are invited to Aura",
        $"Hello {user.Name},\n\nAccept your Aura staff invitation and choose a password by opening this link:\n\n{links.Invitation(token)}\n\nThis invitation expires at {expiresAtUtc:u}.");
}
