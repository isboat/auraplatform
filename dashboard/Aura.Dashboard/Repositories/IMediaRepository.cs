using Aura.Dashboard.Domain;
namespace Aura.Dashboard.Repositories;
public interface IMediaRepository
{
    Task<PageResult<ManagedMedia>> SearchAsync(string? query, string? type, string? status, DateTime? from, DateTime? to, int page, int pageSize);
    Task<ManagedMedia?> FindAsync(string id);
    Task<bool> TrySetReviewStatusAsync(string id, string expected, string next);
    Task DeleteAsync(string id);
    Task<Dictionary<string, long>> CountsAsync();
}
