using Aura.Api.Models;

namespace Aura.Api.Repositories;

public interface IUserRepository
{
    Task<UserDocument?> FindByEmailAsync(string email);
    Task<UserDocument?> FindByIdAsync(string id);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(UserDocument user);
    Task<bool> VerifyAsync(string token);
    Task SetPasswordResetTokenAsync(string userId, string tokenHash, DateTime expiresAt);
    Task<bool> ResetPasswordAsync(string tokenHash, DateTime now, string passwordHash);
    Task<bool> IsSessionValidAsync(string userId, int sessionVersion);
}

public interface IMediaRepository
{
    Task<IReadOnlyList<MediaDocument>> LatestApprovedAsync(int limit);
    Task<IReadOnlyList<MediaDocument>> MostViewedAsync(int limit);
    Task<IReadOnlyList<MediaDocument>> MostLikedAsync(int limit);
    Task<IReadOnlyList<MediaDocument>> SearchAsync(string? tag, int limit);
    Task<MediaDocument?> FindByIdAsync(string id);
    Task<MediaDocument?> FindOwnedAsync(string id, string ownerId);
    Task<IReadOnlyList<MediaDocument>> FindByOwnerAsync(string ownerId);
    Task AddAsync(MediaDocument media);
    Task IncrementViewsAsync(string id);
    Task<bool> DeleteOwnedAsync(string id, string ownerId);
    Task<bool> SetReactionAsync(string id, string userId, bool like);
    Task<bool> SetReviewStatusAsync(string id, string status);
}

public interface ICommentRepository
{
    Task<long> CountAsync(string mediaId);
    Task<IReadOnlyList<CommentDocument>> ListAsync(string mediaId, int limit, DateTime? before);
    Task AddAsync(CommentDocument comment);
    Task DeleteForMediaAsync(string mediaId);
}

public interface IConfigurationRepository
{
    Task<PlatformConfiguration> GetAsync();
    Task SaveAsync(PlatformConfiguration configuration);
}
