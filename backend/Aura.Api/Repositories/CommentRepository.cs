using Aura.Api.Models;
using Aura.Api.Services;
using MongoDB.Driver;

namespace Aura.Api.Repositories;

public sealed class CommentRepository(MongoContext db) : ICommentRepository
{
    public Task<long> CountAsync(string mediaId) => db.Comments.CountDocumentsAsync(x => x.MediaId == mediaId);
    public async Task<IReadOnlyList<CommentDocument>> ListAsync(string mediaId, int limit, DateTime? before)
    {
        var filter = Builders<CommentDocument>.Filter.Eq(x => x.MediaId, mediaId);
        if (before.HasValue) filter &= Builders<CommentDocument>.Filter.Lt(x => x.CreatedAt, before.Value);
        return await db.Comments.Find(filter).SortByDescending(x => x.CreatedAt).Limit(limit).ToListAsync();
    }
    public Task AddAsync(CommentDocument comment) => db.Comments.InsertOneAsync(comment);
    public Task DeleteForMediaAsync(string mediaId) => db.Comments.DeleteManyAsync(x => x.MediaId == mediaId);
}
