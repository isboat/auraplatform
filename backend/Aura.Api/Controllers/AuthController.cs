using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAuthService auth, AuthLinkBuilder links) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<MessageResponse>> Register(RegisterRequest request)
    {
        return Accepted(await auth.RegisterAsync(request, links.VerificationBaseUrl));
    }

    [HttpGet("verify")]
    public async Task<ActionResult<MessageResponse>> Verify(string token) => Ok(await auth.VerifyAsync(token));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) => Ok(await auth.LoginAsync(request));

    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request)
    {
        return Accepted(await auth.ForgotPasswordAsync(request, links.PasswordResetBaseUrl));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request) => Ok(await auth.ResetPasswordAsync(request));
}
