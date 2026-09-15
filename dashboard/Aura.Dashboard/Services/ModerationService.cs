using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class ModerationService(IMediaRepository media,IAuditRepository audit):IModerationService
{
    public async Task DecideAsync(string id,bool approve,string? reason,StaffActor actor)
    {
        if(!approve&&string.IsNullOrWhiteSpace(reason))throw new DashboardRuleException("A rejection reason is required.");
        var item=await media.FindAsync(id)??throw new DashboardRuleException("Media was not found."); var next=approve?ReviewStates.Approved:ReviewStates.Rejected;
        if(!await media.TrySetReviewStatusAsync(id,ReviewStates.InReview,next))throw new ConcurrencyException("This upload has already been reviewed.");
        await audit.AppendAsync(Event(approve?"MediaApproved":"MediaRejected",actor,"Media",id,item.Title,ReviewStates.InReview,next,reason));
    }
    internal static AuditEvent Event(string type,StaffActor a,string targetType,string id,string display,string? old=null,string? next=null,string? reason=null)=>new(){EventType=type,ActorId=a.Id,ActorName=a.Name,ActorEmail=a.Email,ActorRole=a.Role,TargetType=targetType,TargetId=id,TargetDisplay=display,PreviousStatus=old,NewStatus=next,Reason=reason,CorrelationId=a.CorrelationId};
}
