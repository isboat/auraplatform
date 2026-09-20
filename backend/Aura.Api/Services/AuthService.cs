using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Aura.Api.Services;

public sealed class AuthService(IUserRepository users, IConfigurationService configuration, IPasswordHasher passwords, ITokenService tokens, IEmailService email, IClock clock) : IAuthService
{
    private const string ResetRequestedMessage = "If an account exists for that verified email address, a password reset link has been sent.";
    public async Task<MessageResponse> RegisterAsync(RegisterRequest request, string verificationBaseUrl, ClientMetadata metadata)
    {
        if (!(await configuration.GetAsync()).RegistrationEnabled) throw new ServiceException(403, "Registration is currently unavailable.");
        var normalized = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password) || !normalized.Contains('@')) throw new ServiceException(400, "Name, a valid email, and password are required.");
        if (await users.EmailExistsAsync(normalized)) throw new ServiceException(409, "An account with this email already exists.");
        var user = new UserDocument
        {
            Name = request.Name.Trim(),
            Email = normalized,
            PasswordHash = passwords.Hash(request.Password),
            CreatedAt = clock.UtcNow,
            RegistrationMetadata = metadata
        };
        await users.AddAsync(user);
        try
        {
            await email.SendVerificationAsync(user.Email, $"{verificationBaseUrl}?token={user.VerificationToken}");
        }
        catch
        {
            if (user.Id is not null)
                await users.DeleteAsync(user.Id);
            throw;
        }
        return new("Check your email to complete account setup.");
    }

    public async Task<MessageResponse> VerifyAsync(string token)
    {
        if (!await users.VerifyAsync(token)) throw new ServiceException(404, "Verification link is invalid or expired.");
        return new("Your account is verified.");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) throw new ServiceException(401, "Email or password is incorrect.");
        if (user.IsBlocked) throw new ServiceException(403, "Your account has been blocked. Please contact customer service at support@auraplatform.com.");
        if (!user.EmailVerified) throw new ServiceException(403, "Verify your email before signing in.");
        return new(tokens.Create(user), new(user.Id!, user.Name, user.Email, user.IsAdministrator));
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string resetBaseUrl)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains('@')) return new(ResetRequestedMessage);

        var user = await users.FindByEmailAsync(normalized);
        if (user is null || !user.EmailVerified) return new(ResetRequestedMessage);

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        await users.SetPasswordResetTokenAsync(user.Id!, HashToken(token), clock.UtcNow.AddHours(1));
        await email.SendPasswordResetAsync(user.Email, $"{resetBaseUrl}?token={Uri.EscapeDataString(token)}");
        return new(ResetRequestedMessage);
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token)) throw new ServiceException(400, "Password reset link is invalid or expired.");
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8) throw new ServiceException(400, "Password must be at least 8 characters.");
        if (request.Password != request.ConfirmPassword) throw new ServiceException(400, "Passwords do not match.");
        if (!await users.ResetPasswordAsync(HashToken(request.Token), clock.UtcNow, passwords.Hash(request.Password)))
            throw new ServiceException(400, "Password reset link is invalid or expired.");
        return new("Your password has been reset. Sign in with your new password.");
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
