using Aura.Dashboard.Domain;
using Aura.Dashboard.Models;
using Aura.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Dashboard.Controllers;

[Authorize(Roles = Roles.Administrator)]
public sealed class ConfigurationController(IConfigurationManagementService service) : ManagementController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var configuration = await service.GetAsync();
        return View(new ConfigurationViewModel
        {
            RegistrationEnabled = configuration.RegistrationEnabled,
            UploadsEnabled = configuration.UploadsEnabled
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConfigurationViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await service.UpdateAsync(model.RegistrationEnabled, model.UploadsEnabled, Actor());
        return Success("Platform configuration updated.", nameof(Index));
    }
}
