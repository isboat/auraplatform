using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Dashboard.Domain;

public static class Roles { public const string Reviewer = "ContentReviewer"; public const string Administrator = "Administrator"; }
public static class ReviewStates { public const string InReview = "InReview"; public const string Approved = "Approved"; public const string Rejected = "Rejected"; }

[BsonIgnoreExtraElements]
public sealed class StaffUser
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsAdministrator { get; set; }
    public List<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int SessionVersion { get; set; }
    [BsonIgnore] public bool IsAdmin => IsAdministrator || Roles.Contains(Domain.Roles.Administrator);
    [BsonIgnore] public bool IsReviewer => IsAdmin || Roles.Contains(Domain.Roles.Reviewer);
}

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

public sealed record PageResult<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize)
{ public int PageCount => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize)); }
public sealed record StaffActor(string Id, string Name, string Email, string Role, string CorrelationId);
public sealed record DashboardMetrics(long TotalMedia, long Videos, long Images, long Audio, long Pending, long Approved, long Rejected, long TotalUsers, long VerifiedUsers, long BlockedUsers, long Reviewers, long Administrators);
