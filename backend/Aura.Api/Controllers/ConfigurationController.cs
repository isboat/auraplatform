using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Route("api/configuration")]
public sealed class ConfigurationController(IConfigurationService configuration) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<PlatformConfiguration>> Get() => Ok(await configuration.GetAsync());
    [Authorize(Roles = "Admin"), HttpPut] public async Task<ActionResult<PlatformConfiguration>> Update(ConfigurationRequest request) => Ok(await configuration.UpdateAsync(request));
}
