using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Dashboard.Controllers;

[Authorize(Roles = Roles.Administrator)]
public sealed class PlatformUsersController(IPlatformUserRepository users) : Controller
{
    public async Task<IActionResult> Index(string? query, bool? verified, int page = 1) =>
        View(await users.SearchAsync(query, verified, Math.Max(page, 1), 20));

    public async Task<IActionResult> Details(string id)
    {
        var user = await users.FindAsync(id);
        return user is null ? NotFound() : View(user);
    }
}
