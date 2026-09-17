using Aura.Dashboard.Domain;
namespace Aura.Dashboard.Repositories;

public interface IUserRepository
{
    Task<StaffUser?> FindAsync(string id);
    Task<StaffUser?> FindByEmailAsync(string email);
    Task<bool> HasAdministratorAsync();
    Task<bool> TryAcquireFirstAdministratorBootstrapAsync();
    Task ReleaseFirstAdministratorBootstrapAsync();
    Task<PageResult<StaffUser>> SearchAsync(string? query, string? role, bool? blocked, int page, int pageSize);
    Task SaveAsync(StaffUser user);
    Task<bool> AcceptInvitationAsync(string tokenHash, DateTime now, string passwordHash);
    Task<bool> ResetPasswordAsync(string tokenHash, DateTime now, string passwordHash);
    Task DeleteAsync(string id);
    Task<Dictionary<string, long>> CountsAsync();
}
