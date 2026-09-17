namespace Aura.Api.Services;

public sealed class YahooEmailService(IEmailSender sender) : IEmailService
{
    public Task SendVerificationAsync(string email, string verificationUrl) => sender.SendAsync(
        email,
        "Verify your Aura account",
        $"Welcome to Aura. Verify your email address by opening this link:\n\n{verificationUrl}\n\nIf you did not create this account, you can ignore this message.");

    public Task SendPasswordResetAsync(string email, string resetUrl) => sender.SendAsync(
        email,
        "Reset your Aura password",
        $"Reset your Aura password by opening this link:\n\n{resetUrl}\n\nThis link expires in one hour. If you did not request a reset, you can ignore this message.");
}
