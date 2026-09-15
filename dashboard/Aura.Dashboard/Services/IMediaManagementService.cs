using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public interface IMediaManagementService { Task DeleteAsync(string id, string reason, StaffActor actor); }
