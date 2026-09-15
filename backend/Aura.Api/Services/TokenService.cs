using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aura.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Aura.Api.Services;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public string Create(UserDocument user)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id!), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.IsAdministrator ? "Admin" : "User"), new Claim("session_version", user.SessionVersion.ToString()) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));
    }
}
