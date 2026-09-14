using Aura.Api.Models;

namespace Aura.Api.Tests.Fakes;

internal static class TestData
{
    public static UserDocument User(bool verified = true) => new() { Id = "507f1f77bcf86cd799439011", Name = "Aura User", Email = "user@example.com", PasswordHash = "hash", EmailVerified = verified };
    public static MediaDocument Media(string status = "Approved") => new() { Id = "507f191e810c19729de860ea", OwnerId = "507f1f77bcf86cd799439011", OwnerName = "Aura User", Title = "A title", Description = "Description", MediaType = "video", ObjectKey = "owner/key.mp4", ReviewStatus = status, Views = 5, LikeCount = 3, DislikeCount = 1, Tags = ["nature"] };
}
