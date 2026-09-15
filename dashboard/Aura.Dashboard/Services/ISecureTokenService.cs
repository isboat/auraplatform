namespace Aura.Dashboard.Services;

public interface ISecureTokenService
{
    string Create();
    string Hash(string token);
}
