namespace Aura.Api.Models;

public sealed record UserResponse(string Id, string Name, string Email, bool IsAdministrator);
public sealed record AuthResponse(string Token, UserResponse User);
public sealed record MessageResponse(string Message);
public sealed record UploadResponse(string MediaId, string UploadId, IReadOnlyList<string> PartUrls, string Message);
public sealed record MediaResponse(string Id, string Title, string Description, IReadOnlyList<string> Tags, string MediaType, string ReviewStatus, DateTime CreatedAt, string OwnerName, long Views, long Likes, long Dislikes, long Comments, string? Url);
public sealed record HomepageResponse(IReadOnlyList<MediaResponse> Latest, IReadOnlyList<MediaResponse> MostViewed, IReadOnlyList<MediaResponse> MostLiked);
