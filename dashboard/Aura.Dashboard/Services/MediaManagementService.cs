using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class MediaManagementService(IMediaRepository media,IAssetStorage storage,IAuditRepository audit):IMediaManagementService
{public async Task DeleteAsync(string id,string reason,StaffActor actor){if(string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A deletion reason is required.");var item=await media.FindAsync(id)??throw new DashboardRuleException("Media was not found.");await storage.DeleteAsync(item.ObjectKey);await media.DeleteAsync(id);await audit.AppendAsync(ModerationService.Event("MediaDeleted",actor,"Media",id,item.Title,reason:reason));}}
