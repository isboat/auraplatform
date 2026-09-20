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
            ["UploadMetadata"] = new BsonDocument
            {
                ["IpAddress"] = "203.0.113.9",
                ["Browser"] = "Firefox 143.0",
                ["Device"] = "Desktop",
                ["Country"] = "GB"
            },
            ["Views"] = 42,
            ["LikeCount"] = 8,
            ["DislikeCount"] = 1,
            ["Likes"] = new BsonArray { "user-1" },
            ["Dislikes"] = new BsonArray()
        };

        var media = BsonSerializer.Deserialize<ManagedMedia>(document);

        Assert.Equal("Compatible media", media.Title);
        Assert.Equal("media/video.mp4", media.ObjectKey);
        Assert.Equal("203.0.113.9", media.UploadMetadata!.IpAddress);
        Assert.Equal("Firefox 143.0", media.UploadMetadata.Browser);
        Assert.Equal("Desktop", media.UploadMetadata.Device);
        Assert.Equal("GB", media.UploadMetadata.Country);
    }

    [Fact]
    public void Staff_user_ignores_fields_added_by_other_identity_workflows()
    {
        var registeredAt = new DateTime(2026, 9, 20, 14, 30, 0, DateTimeKind.Utc);
        var document = new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["Name"] = "Administrator",
            ["Email"] = "admin@example.com",
            ["PasswordHash"] = "hash",
            ["CreatedAt"] = registeredAt,
            ["RegistrationMetadata"] = new BsonDocument
            {
                ["IpAddress"] = "203.0.113.10",
                ["Browser"] = "Firefox 143.0",
                ["Device"] = "Desktop",
                ["Country"] = "GB",
                ["Region"] = "ENG",
                ["City"] = "London",
                ["TimeZone"] = "Europe/London"
            },
            ["Roles"] = new BsonArray { Roles.Administrator },
            ["PasswordResetTokenHash"] = "not-loaded-by-dashboard",
            ["VerificationToken"] = "not-loaded-by-dashboard"
        };

        var user = BsonSerializer.Deserialize<StaffUser>(document);

        Assert.Equal("admin@example.com", user.Email);
        Assert.True(user.IsAdmin);
        Assert.Equal(registeredAt, user.CreatedAt);
    }

    [Fact]
    public void Platform_user_reads_backend_registration_fields()
    {
        var registeredAt = new DateTime(2026, 9, 20, 14, 30, 0, DateTimeKind.Utc);
        var document = new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["Name"] = "Platform User",
            ["Email"] = "user@example.com",
            ["PasswordHash"] = "not-loaded-by-dashboard",
            ["EmailVerified"] = true,
            ["CreatedAt"] = registeredAt,
            ["RegistrationMetadata"] = new BsonDocument
            {
                ["IpAddress"] = "203.0.113.10",
                ["Browser"] = "Firefox 143.0",
                ["Device"] = "Desktop",
                ["Country"] = "GB",
                ["Region"] = "ENG",
                ["City"] = "London",
                ["TimeZone"] = "Europe/London"
            }
        };

        var user = BsonSerializer.Deserialize<PlatformUser>(document);

        Assert.Equal("user@example.com", user.Email);
        Assert.True(user.EmailVerified);
        Assert.Equal(registeredAt, user.CreatedAt);
        Assert.Equal("203.0.113.10", user.RegistrationMetadata!.IpAddress);
        Assert.Equal("Firefox 143.0", user.RegistrationMetadata.Browser);
        Assert.Equal("Desktop", user.RegistrationMetadata.Device);
        Assert.Equal("London", user.RegistrationMetadata.City);
        Assert.Equal("Europe/London", user.RegistrationMetadata.TimeZone);
    }
}
