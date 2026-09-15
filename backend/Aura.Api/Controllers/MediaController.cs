using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Route("api/media")]
public sealed class MediaController(IMediaService media, IUploadService uploads) : ControllerBase
{
    [HttpGet("home")] public async Task<ActionResult<HomepageResponse>> Home() => Ok(await media.HomeAsync());
    [HttpGet("search")] public async Task<ActionResult<IReadOnlyList<MediaResponse>>> Search(string? tag) => Ok(await media.SearchAsync(tag));
    [HttpGet("{id}")] public async Task<ActionResult<MediaResponse>> Get(string id) => Ok(await media.GetAsync(id));
    [Authorize, HttpGet("mine")] public async Task<ActionResult<IReadOnlyList<MediaResponse>>> Mine() => Ok(await media.MineAsync(User.UserId()));
    [Authorize, HttpPost("uploads")] public async Task<ActionResult<UploadResponse>> BeginUpload(UploadRequest request) => Ok(await uploads.BeginAsync(request, User.UserId()));
    [Authorize, HttpPost("{id}/complete")] public async Task<ActionResult<MessageResponse>> CompleteUpload(string id, CompleteUploadRequest request) => Ok(await uploads.CompleteAsync(id, request, User.UserId()));
    [Authorize, HttpDelete("{id}")] public async Task<IActionResult> Delete(string id) { await media.DeleteAsync(id, User.UserId()); return NoContent(); }
    [Authorize, HttpPost("{id}/reaction")] public async Task<IActionResult> React(string id, ReactionRequest request) { await media.ReactAsync(id, User.UserId(), request.Like); return NoContent(); }
}
