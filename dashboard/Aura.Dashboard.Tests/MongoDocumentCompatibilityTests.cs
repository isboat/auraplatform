using Aura.Dashboard.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Aura.Dashboard.Tests;

public sealed class MongoDocumentCompatibilityTests
{
    [Fact]
    public void Managed_media_ignores_public_api_fields()
    {
        var document = new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["OwnerId"] = "owner-1",
            ["OwnerName"] = "Uploader",
            ["Title"] = "Compatible media",
            ["MediaType"] = "video",
            ["ObjectKey"] = "media/video.mp4",
            ["Views"] = 42,
            ["LikeCount"] = 8,
            ["DislikeCount"] = 1,
            ["Likes"] = new BsonArray { "user-1" },
            ["Dislikes"] = new BsonArray()
        };

        var media = BsonSerializer.Deserialize<ManagedMedia>(document);

        Assert.Equal("Compatible media", media.Title);
        Assert.Equal("media/video.mp4", media.ObjectKey);
    }

    [Fact]
    public void Staff_user_ignores_fields_added_by_other_identity_workflows()
    {
        var document = new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["Name"] = "Administrator",
            ["Email"] = "admin@example.com",
            ["PasswordHash"] = "hash",
            ["Roles"] = new BsonArray { Roles.Administrator },
            ["PasswordResetTokenHash"] = "not-loaded-by-dashboard",
            ["VerificationToken"] = "not-loaded-by-dashboard"
        };

        var user = BsonSerializer.Deserialize<StaffUser>(document);

        Assert.Equal("admin@example.com", user.Email);
        Assert.True(user.IsAdmin);
    }
}
