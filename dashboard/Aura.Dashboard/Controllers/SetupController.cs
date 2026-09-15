using Aura.Dashboard.Domain;
using Aura.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Dashboard.Controllers;

[AllowAnonymous]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class SetupController(IStaffBootstrapService bootstrap) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!await bootstrap.IsAvailableAsync())
            return NotFound();

        return View(new FirstAdministratorRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(FirstAdministratorRequest request)
    {
        if (!await bootstrap.IsAvailableAsync())
            return NotFound();

        if (!ModelState.IsValid)
            return View(request);

        try
        {
            await bootstrap.CreateFirstAdministratorAsync(request, HttpContext.TraceIdentifier);
            TempData["Success"] = "Administrator created. You can now sign in.";
            return RedirectToAction("Login", "Account");
        }
        catch (DashboardRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(request);
        }
    }
}
