using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;

namespace Aura.Api.Services;

public sealed class AuthService(IUserRepository users, IConfigurationRepository configuration, IPasswordHasher passwords, ITokenService tokens, IEmailService email) : IAuthService
{
    public async Task<MessageResponse> RegisterAsync(RegisterRequest request, string verificationBaseUrl)
    {
        if (!(await configuration.GetAsync()).RegistrationEnabled) throw new ServiceException(403, "Registration is currently unavailable.");
        var normalized = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password) || !normalized.Contains('@')) throw new ServiceException(400, "Name, a valid email, and password are required.");
        if (await users.EmailExistsAsync(normalized)) throw new ServiceException(409, "An account with this email already exists.");
        var user = new UserDocument { Name = request.Name.Trim(), Email = normalized, PasswordHash = passwords.Hash(request.Password) };
        await users.AddAsync(user);
        await email.SendVerificationAsync(user.Email, $"{verificationBaseUrl}?token={user.VerificationToken}");
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
        if (!user.EmailVerified) throw new ServiceException(403, "Verify your email before signing in.");
        return new(tokens.Create(user), new(user.Id!, user.Name, user.Email, user.IsAdministrator));
    }
}
