using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Repositories;

public interface IPlatformUserRepository
{
    Task<PageResult<PlatformUser>> SearchAsync(string? query, bool? verified, int page, int pageSize);
    Task<PlatformUser?> FindAsync(string id);
}
