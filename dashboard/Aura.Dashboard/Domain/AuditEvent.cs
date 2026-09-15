using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Dashboard.Domain;

[BsonIgnoreExtraElements]
public sealed class AuditEvent
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string EventType { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public required string ActorId { get; set; }
    public required string ActorName { get; set; }
    public required string ActorEmail { get; set; }
    public required string ActorRole { get; set; }
    public required string TargetType { get; set; }
    public required string TargetId { get; set; }
    public string? TargetDisplay { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? Reason { get; set; }
    public required string CorrelationId { get; set; }
    public string Outcome { get; set; } = "Succeeded";
}
