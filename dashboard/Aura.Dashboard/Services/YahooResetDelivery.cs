using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class YahooResetDelivery(IEmailSender sender, DashboardLinkBuilder links) : IResetDelivery
{
    public Task SendAsync(StaffUser user, string token, DateTime expiresAtUtc) => sender.SendAsync(
        user.Email,
        "Reset your Aura password",
        $"Hello {user.Name},\n\nReset your Aura password by opening this link:\n\n{links.PasswordReset(token)}\n\nThis link expires at {expiresAtUtc:u}. If you did not request a reset, contact an administrator.");
}
