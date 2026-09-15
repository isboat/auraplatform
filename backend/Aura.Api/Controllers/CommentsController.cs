using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aura.Api.Controllers;

[ApiController, Route("api/media/{mediaId}/comments")]
public sealed class CommentsController(ICommentService comments) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<CommentDocument>>> List(string mediaId, int? limit, DateTime? before) => Ok(await comments.ListAsync(mediaId, limit, before));
    [Authorize, HttpPost]
    public async Task<ActionResult<CommentDocument>> Add(string mediaId, CommentRequest request)
    {
        var comment = await comments.AddAsync(mediaId, request, User.UserId());
        return CreatedAtAction(nameof(List), new { mediaId }, comment);
    }
}
