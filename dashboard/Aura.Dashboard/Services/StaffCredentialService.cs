using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class StaffCredentialService(
    IUserRepository users,
    ISecureTokenService tokens) : IStaffCredentialService
{
    public Task<bool> AcceptInvitationAsync(string token, string password) =>
        users.AcceptInvitationAsync(tokens.Hash(token), DateTime.UtcNow, HashPassword(password));

    public Task<bool> ResetPasswordAsync(string token, string password) =>
        users.ResetPasswordAsync(tokens.Hash(token), DateTime.UtcNow, HashPassword(password));

    private static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
