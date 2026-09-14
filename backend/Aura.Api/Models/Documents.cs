using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Api.Models;

public sealed class UserDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public bool EmailVerified { get; set; }
    public string VerificationToken { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsAdministrator { get; set; }
}

public sealed class MediaDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string OwnerId { get; set; }
    public required string OwnerName { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public required string MediaType { get; set; }
    public required string ObjectKey { get; set; }
    public string ReviewStatus { get; set; } = "InReview";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long Views { get; set; }
    public long LikeCount { get; set; }
    public long DislikeCount { get; set; }
    public HashSet<string> Likes { get; set; } = [];
    public HashSet<string> Dislikes { get; set; } = [];
}

public sealed class CommentDocument
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string MediaId { get; set; }
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class PlatformConfiguration
{
    [BsonId] public string Id { get; set; } = "platform";
    public bool RegistrationEnabled { get; set; } = true;
    public bool UploadsEnabled { get; set; } = true;
}
