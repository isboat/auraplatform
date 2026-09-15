using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Aura.Dashboard.Services;

public sealed class SecureTokenService : ISecureTokenService
{
    public string Create() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public string Hash(string token) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
