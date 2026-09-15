using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public interface IModerationService { Task DecideAsync(string mediaId, bool approve, string? reason, StaffActor actor); }
