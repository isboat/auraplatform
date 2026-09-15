using Aura.Dashboard.Domain;
namespace Aura.Dashboard.Repositories;
public interface IResetDelivery
{
    Task SendAsync(StaffUser user);
}
