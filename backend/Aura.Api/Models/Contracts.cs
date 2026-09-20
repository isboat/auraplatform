namespace Aura.Api.Models;

public record RegisterRequest(string Name, string Email, string Password, string? Phone = null);
public record LoginRequest(string Email, string Password);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string Password, string ConfirmPassword);
public record UploadRequest(string? Title, string Description, List<string>? Tags, string FileName, string ContentType, long FileSize);
public record CompleteUploadRequest(string UploadId, List<UploadedPart> Parts);
public record UploadedPart(int PartNumber, string ETag);
public record CommentRequest(string Body);
public record ReactionRequest(bool Like);
public record ConfigurationRequest(bool RegistrationEnabled, bool UploadsEnabled);
