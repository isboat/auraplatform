using Aura.Dashboard.Domain;
using Aura.Dashboard.Models;
using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Dashboard.Controllers;

[Authorize(Roles = Roles.Administrator)]
public sealed class MediaController(
    IMediaRepository media,
    IMediaManagementService service,
    IAssetStorage storage) : ManagementController
{
    public async Task<IActionResult> Index(string? query, string? type, string? status, int page = 1) =>
        View(await media.SearchAsync(query, type, status, null, null, Math.Max(page, 1), 20));

    public async Task<IActionResult> Details(string id)
    {
        var item = await media.FindAsync(id);
        if (item is null) return NotFound();

        return View(new MediaDetailsViewModel(item, storage.ReadUrl(item.ObjectKey)));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, string reason)
    {
        try
        {
            await service.DeleteAsync(id, reason, Actor());
            return Success("Media and its stored asset were deleted.", nameof(Index));
        }
        catch (DashboardRuleException exception)
        {
            return Failure(exception, nameof(Index));
        }
    }
}
