using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Dashboard.Domain;

[BsonIgnoreExtraElements]
public sealed class PlatformUser
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)] public string? Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClientMetadata? RegistrationMetadata { get; set; }
}
