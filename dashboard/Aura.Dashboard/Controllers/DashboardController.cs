using Aura.Dashboard.Domain; using Aura.Dashboard.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace Aura.Dashboard.Controllers;
[Authorize(Roles=Roles.Reviewer+","+Roles.Administrator)] public sealed class DashboardController(IReportingService reports):Controller { public async Task<IActionResult> Index()=>View(await reports.GetAsync()); }
