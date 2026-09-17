using System.Security.Claims;
using Aura.Dashboard.Domain;
using Aura.Dashboard.Models;
using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Aura.Dashboard.Controllers;

public sealed class AccountController(
    IUserRepository users,
    IStaffCredentialService credentials) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login() => View(new LoginModel("", ""));

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("staff-login")]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
    {
        var user = await users.FindByEmailAsync(model.Email);
        if (user is null || !user.EmailVerified || user.IsBlocked || !user.IsReviewer || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid credentials or dashboard access is unavailable.");
            return View(model);
        }

        var role = user.IsAdmin ? Roles.Administrator : Roles.Reviewer;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim("session_version", user.SessionVersion.ToString())
        };
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = false });
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }

    [AllowAnonymous, HttpGet("account/accept-invitation")]
    public IActionResult AcceptInvitation(string token) => View(new AcceptInvitationViewModel { Token = token });

    [AllowAnonymous, HttpPost("account/accept-invitation"), ValidateAntiForgeryToken, EnableRateLimiting("staff-login")]
    public async Task<IActionResult> AcceptInvitation(AcceptInvitationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!await credentials.AcceptInvitationAsync(model.Token, model.Password))
        {
            ModelState.AddModelError("", "This invitation link is invalid or expired.");
            return View(model);
        }

        TempData["Success"] = "Your invitation was accepted. Sign in with your new password.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous, HttpGet("account/reset-password")]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordViewModel { Token = token });

    [AllowAnonymous, HttpPost("account/reset-password"), ValidateAntiForgeryToken, EnableRateLimiting("staff-login")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!await credentials.ResetPasswordAsync(model.Token, model.Password))
        {
            ModelState.AddModelError("", "This password reset link is invalid or expired.");
            return View(model);
        }

        TempData["Success"] = "Your password was reset. Sign in with your new password.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
