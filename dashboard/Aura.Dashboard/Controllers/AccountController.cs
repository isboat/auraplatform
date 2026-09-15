using System.Security.Claims;
using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Aura.Dashboard.Controllers;
public sealed record LoginModel(string Email,string Password);
public sealed class AccountController(IUserRepository users):Controller
{
    [AllowAnonymous,HttpGet] public IActionResult Login()=>View(new LoginModel("",""));
    [AllowAnonymous,HttpPost,ValidateAntiForgeryToken,EnableRateLimiting("staff-login")] public async Task<IActionResult> Login(LoginModel model,string? returnUrl=null){var user=await users.FindByEmailAsync(model.Email);if(user is null||!user.EmailVerified||user.IsBlocked||!user.IsReviewer||!BCrypt.Net.BCrypt.Verify(model.Password,user.PasswordHash)){ModelState.AddModelError("","Invalid credentials or dashboard access is unavailable.");return View(model);}var role=user.IsAdmin?Roles.Administrator:Roles.Reviewer;var claims=new[]{new Claim(ClaimTypes.NameIdentifier,user.Id!),new Claim(ClaimTypes.Name,user.Name),new Claim(ClaimTypes.Email,user.Email),new Claim(ClaimTypes.Role,role),new Claim("session_version",user.SessionVersion.ToString())};await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)),new AuthenticationProperties{IsPersistent=false});return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl:"/");}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return RedirectToAction(nameof(Login));}
    [AllowAnonymous] public IActionResult AccessDenied()=>View();
}
