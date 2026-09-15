using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;

namespace Aura.Api.Services;

public sealed class MediaService(IMediaRepository media, ICommentRepository comments, IMediaStorage storage) : IMediaService
{
    private async Task<MediaResponse> ViewAsync(MediaDocument item, bool includeCommentCount = true) => new(item.Id!, item.Title, item.Description, item.Tags, item.MediaType, item.ReviewStatus, item.CreatedAt, item.OwnerName, item.Views, item.LikeCount, item.DislikeCount, includeCommentCount ? await comments.CountAsync(item.Id!) : 0, item.ReviewStatus == "Approved" ? storage.ReadUrl(item.ObjectKey) : null);
    private async Task<IReadOnlyList<MediaResponse>> ViewsAsync(IReadOnlyList<MediaDocument> items, bool includeCommentCount = false)
    {
        var responses = new List<MediaResponse>();
        foreach (var item in items) responses.Add(await ViewAsync(item, includeCommentCount));
        return responses;
    }
    public async Task<HomepageResponse> HomeAsync() => new(await ViewsAsync(await media.LatestApprovedAsync(10)), await ViewsAsync(await media.MostViewedAsync(10)), await ViewsAsync(await media.MostLikedAsync(10)));
    public async Task<IReadOnlyList<MediaResponse>> SearchAsync(string? tag) => await ViewsAsync(await media.SearchAsync(tag, 50));
    public async Task<MediaResponse> GetAsync(string id)
    {
        var item = await media.FindByIdAsync(id) ?? throw new ServiceException(404, "Media was not found.");
        await media.IncrementViewsAsync(id);
        item.Views++;
        return await ViewAsync(item);
    }
    public async Task<IReadOnlyList<MediaResponse>> MineAsync(string userId) => await ViewsAsync(await media.FindByOwnerAsync(userId), true);
    public async Task DeleteAsync(string id, string userId)
    {
        var item = await media.FindOwnedAsync(id, userId) ?? throw new ServiceException(404, "Media was not found.");
        if (!await media.DeleteOwnedAsync(id, userId)) throw new ServiceException(404, "Media was not found.");
        await storage.DeleteAsync(item.ObjectKey);
        await comments.DeleteForMediaAsync(id);
    }
    public async Task ReactAsync(string id, string userId, bool like)
    {
        if (!await media.SetReactionAsync(id, userId, like)) throw new ServiceException(404, "Approved media was not found.");
    }
    public async Task ReviewAsync(string id, string status)
    {
        if (status is not ("Approved" or "Rejected")) throw new ServiceException(400, "Review status must be Approved or Rejected.");
        if (!await media.SetReviewStatusAsync(id, status)) throw new ServiceException(404, "Media was not found.");
    }
}
