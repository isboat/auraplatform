using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Repositories;
public interface IMediaRepository
{
    Task<PageResult<ManagedMedia>> SearchAsync(string? query, string? type, string? status, DateTime? from, DateTime? to, int page, int pageSize);
    Task<ManagedMedia?> FindAsync(string id);
    Task<bool> TrySetReviewStatusAsync(string id, string expected, string next);
    Task DeleteAsync(string id);
    Task<Dictionary<string,long>> CountsAsync();
}
public interface IUserRepository
{
    Task<StaffUser?> FindAsync(string id); Task<StaffUser?> FindByEmailAsync(string email);
    Task<PageResult<StaffUser>> SearchAsync(string? query, string? role, bool? blocked, int page, int pageSize);
    Task SaveAsync(StaffUser user); Task DeleteAsync(string id); Task<Dictionary<string,long>> CountsAsync();
}
public interface IAuditRepository { Task AppendAsync(AuditEvent entry); Task<PageResult<AuditEvent>> SearchAsync(string? type, string? actor, string? target, string? outcome, DateTime? from, DateTime? to, int page, int pageSize); }
public interface IAssetStorage { Task DeleteAsync(string objectKey); string ReadUrl(string objectKey); }
public interface IResetDelivery { Task SendAsync(StaffUser user); }
