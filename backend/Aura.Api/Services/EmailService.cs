namespace Aura.Api.Services;

public interface IEmailService
{
    Task SendVerificationAsync(string email, string verificationUrl);
    Task SendPasswordResetAsync(string email, string resetUrl);
}
public sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendVerificationAsync(string email, string verificationUrl) { logger.LogInformation("Verification email for {Email}: {Url}", email, verificationUrl); return Task.CompletedTask; }
    public Task SendPasswordResetAsync(string email, string resetUrl) { logger.LogInformation("Password reset email for {Email}: {Url}", email, resetUrl); return Task.CompletedTask; }
}
