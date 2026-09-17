namespace Aura.Api.Services;

public interface IEmailService
{
    Task SendVerificationAsync(string email, string verificationUrl);
    Task SendPasswordResetAsync(string email, string resetUrl);
}
