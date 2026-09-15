using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Dashboard.Domain;

[BsonIgnoreExtraElements]
public sealed class ManagedMedia
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string OwnerId { get; set; }
    public required string OwnerName { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public required string MediaType { get; set; }
    public string ContentType { get; set; } = "application/octet-stream";
    public required string ObjectKey { get; set; }
    public string ReviewStatus { get; set; } = ReviewStates.InReview;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
