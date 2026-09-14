using Aura.Api.Models;
using Aura.Api.Services;
using MongoDB.Driver;

namespace Aura.Api.Repositories;

public sealed class MediaRepository(MongoContext db) : IMediaRepository
{
    private FilterDefinition<MediaDocument> Approved => Builders<MediaDocument>.Filter.Eq(x => x.ReviewStatus, "Approved");
    private async Task<IReadOnlyList<MediaDocument>> ApprovedByAsync(SortDefinition<MediaDocument> sort, int limit) => await db.Media.Find(Approved).Sort(sort).Limit(limit).ToListAsync();
    public Task<IReadOnlyList<MediaDocument>> LatestApprovedAsync(int limit) => ApprovedByAsync(Builders<MediaDocument>.Sort.Descending(x => x.CreatedAt), limit);
    public Task<IReadOnlyList<MediaDocument>> MostViewedAsync(int limit) => ApprovedByAsync(Builders<MediaDocument>.Sort.Descending(x => x.Views), limit);
    public Task<IReadOnlyList<MediaDocument>> MostLikedAsync(int limit) => ApprovedByAsync(Builders<MediaDocument>.Sort.Descending(x => x.LikeCount), limit);
    public async Task<IReadOnlyList<MediaDocument>> SearchAsync(string? tag, int limit)
    {
        var filter = string.IsNullOrWhiteSpace(tag) ? Builders<MediaDocument>.Filter.Empty : Builders<MediaDocument>.Filter.AnyEq(x => x.Tags, tag.Trim().ToLowerInvariant());
        return await db.Media.Find(filter).SortByDescending(x => x.CreatedAt).Limit(limit).ToListAsync();
    }
    public async Task<MediaDocument?> FindByIdAsync(string id) => await db.Media.Find(x => x.Id == id).FirstOrDefaultAsync();
    public async Task<MediaDocument?> FindOwnedAsync(string id, string ownerId) => await db.Media.Find(x => x.Id == id && x.OwnerId == ownerId).FirstOrDefaultAsync();
    public async Task<IReadOnlyList<MediaDocument>> FindByOwnerAsync(string ownerId) => await db.Media.Find(x => x.OwnerId == ownerId).SortByDescending(x => x.CreatedAt).ToListAsync();
    public Task AddAsync(MediaDocument media) => db.Media.InsertOneAsync(media);
    public Task IncrementViewsAsync(string id) => db.Media.UpdateOneAsync(x => x.Id == id, Builders<MediaDocument>.Update.Inc(x => x.Views, 1));
    public async Task<bool> DeleteOwnedAsync(string id, string ownerId) => (await db.Media.DeleteOneAsync(x => x.Id == id && x.OwnerId == ownerId)).DeletedCount == 1;
    public async Task<bool> SetReactionAsync(string id, string userId, bool like)
    {
        var media = await FindByIdAsync(id);
        if (media is null || media.ReviewStatus != "Approved") return false;
        var hadLike = media.Likes.Contains(userId);
        var hadDislike = media.Dislikes.Contains(userId);
        var update = like
            ? Builders<MediaDocument>.Update.AddToSet(x => x.Likes, userId).Pull(x => x.Dislikes, userId).Inc(x => x.LikeCount, hadLike ? 0 : 1).Inc(x => x.DislikeCount, hadDislike ? -1 : 0)
            : Builders<MediaDocument>.Update.AddToSet(x => x.Dislikes, userId).Pull(x => x.Likes, userId).Inc(x => x.DislikeCount, hadDislike ? 0 : 1).Inc(x => x.LikeCount, hadLike ? -1 : 0);
        return (await db.Media.UpdateOneAsync(x => x.Id == id && x.ReviewStatus == "Approved", update)).MatchedCount == 1;
    }
    public async Task<bool> SetReviewStatusAsync(string id, string status) => (await db.Media.UpdateOneAsync(x => x.Id == id, Builders<MediaDocument>.Update.Set(x => x.ReviewStatus, status))).MatchedCount == 1;
}
