using Aura.Dashboard.Domain;
using MongoDB.Driver;

namespace Aura.Dashboard.Repositories;

public sealed class MongoPlatformUserRepository(MongoContext context) : IPlatformUserRepository
{
    public async Task<PageResult<PlatformUser>> SearchAsync(string? query, bool? verified, int page, int pageSize)
    {
        var filters = Builders<PlatformUser>.Filter;
        var values = new List<FilterDefinition<PlatformUser>>();
        if (!string.IsNullOrWhiteSpace(query))
            values.Add(filters.Or(
                filters.Regex(user => user.Name, new(query, "i")),
                filters.Regex(user => user.Email, new(query, "i"))));
        if (verified.HasValue)
            values.Add(filters.Eq(user => user.EmailVerified, verified.Value));

        var filter = values.Count == 0 ? filters.Empty : filters.And(values);
        var total = await context.PlatformUsers.CountDocumentsAsync(filter);
        var items = await context.PlatformUsers.Find(filter)
            .SortByDescending(user => user.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return new(items, total, page, pageSize);
    }

    public Task<PlatformUser?> FindAsync(string id) =>
        context.PlatformUsers.Find(user => user.Id == id).FirstOrDefaultAsync();
}
