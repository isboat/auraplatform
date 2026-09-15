using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Services;

public interface IStaffBootstrapService
{
    Task<bool> IsAvailableAsync();
    Task<StaffUser> CreateFirstAdministratorAsync(FirstAdministratorRequest request, string correlationId);
}
