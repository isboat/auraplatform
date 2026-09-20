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
    public UploadMetadata? UploadMetadata { get; set; }
    public string ReviewStatus { get; set; } = ReviewStates.InReview;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class UploadMetadata
{
    public string? IpAddress { get; set; }
    public string? Browser { get; set; }
    public string? Device { get; set; }
    public string? Platform { get; set; }
    public string? UserAgent { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
    public string? TimeZone { get; set; }
}
