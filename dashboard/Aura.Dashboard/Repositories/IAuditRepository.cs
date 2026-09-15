using Aura.Dashboard.Domain;
namespace Aura.Dashboard.Repositories;
public interface IAuditRepository
{
    Task AppendAsync(AuditEvent entry);
    Task<PageResult<AuditEvent>> SearchAsync(string? type, string? actor, string? target, string? outcome, DateTime? from, DateTime? to, int page, int pageSize);
}
