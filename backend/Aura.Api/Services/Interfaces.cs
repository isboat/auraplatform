using Aura.Api.Models;

namespace Aura.Api.Services;

public interface ITokenService { string Create(UserDocument user); }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string hash); }
public interface IClock { DateTime UtcNow { get; } }
public interface IMediaStorage
{
    Task<(string UploadId, IReadOnlyList<string> Urls)> BeginAsync(string key, string contentType, long size);
    Task CompleteAsync(string key, string contentType, string uploadId, IReadOnlyList<UploadedPart> parts);
    Task DeleteAsync(string key);
    string ReadUrl(string key);
    Task CheckHealthAsync(CancellationToken cancellationToken);
}
public interface IAuthService
{
    Task<MessageResponse> RegisterAsync(RegisterRequest request, string verificationBaseUrl);
    Task<MessageResponse> VerifyAsync(string token);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
public interface IConfigurationService { Task<PlatformConfiguration> GetAsync(); Task<PlatformConfiguration> UpdateAsync(ConfigurationRequest request); }
public interface IMediaService
{
    Task<HomepageResponse> HomeAsync();
    Task<IReadOnlyList<MediaResponse>> SearchAsync(string? tag);
    Task<MediaResponse> GetAsync(string id);
    Task<IReadOnlyList<MediaResponse>> MineAsync(string userId);
    Task DeleteAsync(string id, string userId);
    Task ReactAsync(string id, string userId, bool like);
    Task ReviewAsync(string id, string status);
}
public interface IUploadService
{
    Task<UploadResponse> BeginAsync(UploadRequest request, string userId);
    Task<MessageResponse> CompleteAsync(string id, CompleteUploadRequest request, string userId);
}
public interface ICommentService
{
    Task<IReadOnlyList<CommentDocument>> ListAsync(string mediaId, int? limit, DateTime? before);
    Task<CommentDocument> AddAsync(string mediaId, CommentRequest request, string userId);
}
