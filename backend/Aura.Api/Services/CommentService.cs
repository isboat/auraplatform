using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;

namespace Aura.Api.Services;

public sealed class CommentService(ICommentRepository comments, IUserRepository users, IMediaRepository media) : ICommentService
{
    public Task<IReadOnlyList<CommentDocument>> ListAsync(string mediaId, int? limit, DateTime? before) => comments.ListAsync(mediaId, Math.Clamp(limit ?? 10, 1, 50), before);
    public async Task<CommentDocument> AddAsync(string mediaId, CommentRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(request.Body)) throw new ServiceException(400, "Comment is required.");
        if (await media.FindByIdAsync(mediaId) is null) throw new ServiceException(404, "Media was not found.");
        var user = await users.FindByIdAsync(userId) ?? throw new ServiceException(401, "User account was not found.");
        var comment = new CommentDocument { MediaId = mediaId, UserId = user.Id!, UserName = user.Name, Body = request.Body.Trim() };
        await comments.AddAsync(comment);
        return comment;
    }
}
