namespace Aura.Dashboard.Services;

public interface IStaffCredentialService
{
    Task<bool> AcceptInvitationAsync(string token, string password);
    Task<bool> ResetPasswordAsync(string token, string password);
}
