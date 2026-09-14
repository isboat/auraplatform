using Aura.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Authorize(Roles = "Admin"), Route("api/admin/media")]
public sealed class AdminMediaController(IMediaService media) : ControllerBase
{
    [HttpPut("{id}/review")]
    public async Task<IActionResult> Review(string id, string status) { await media.ReviewAsync(id, status); return NoContent(); }
}
