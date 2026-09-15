using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Repositories;

public interface IStaffInvitationDelivery
{
    Task SendAsync(StaffUser user, string token, DateTime expiresAtUtc);
}
