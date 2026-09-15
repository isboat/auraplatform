using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthService auth, IConfiguration configuration) : ControllerBase
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

    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request)
    {
        var frontendUrl = (configuration["FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
        return Accepted(await auth.ForgotPasswordAsync(request, $"{frontendUrl}/reset-password"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request) => Ok(await auth.ResetPasswordAsync(request));
}
