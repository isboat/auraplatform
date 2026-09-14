using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<MessageResponse>> Register(RegisterRequest request)
    {
        var verificationUrl = Url.ActionLink(nameof(Verify), values: new { token = "" })?.Split('?')[0] ?? $"{Request.Scheme}://{Request.Host}/api/auth/verify";
        return Accepted(await auth.RegisterAsync(request, verificationUrl));
    }

    [HttpGet("verify")]
    public async Task<ActionResult<MessageResponse>> Verify(string token) => Ok(await auth.VerifyAsync(token));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) => Ok(await auth.LoginAsync(request));
}
