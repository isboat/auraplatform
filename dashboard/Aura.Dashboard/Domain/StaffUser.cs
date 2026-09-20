using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Dashboard.Domain;

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
    public ClientMetadata? RegistrationMetadata { get; set; }
    public int SessionVersion { get; set; }
    public string? InvitationTokenHash { get; set; }
    public DateTime? InvitationExpiresAtUtc { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetExpiresAtUtc { get; set; }
    [BsonIgnore] public bool IsAdmin => IsAdministrator || Roles.Contains(Domain.Roles.Administrator);
    [BsonIgnore] public bool IsReviewer => IsAdmin || Roles.Contains(Domain.Roles.Reviewer);
}
