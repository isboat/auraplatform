namespace Aura.Api.Services;
public interface IEmailService { Task SendVerificationAsync(string email, string verificationUrl); }
public sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendVerificationAsync(string email, string verificationUrl) { logger.LogInformation("Verification email for {Email}: {Url}", email, verificationUrl); return Task.CompletedTask; }
}
