using Aura.Api.Models;
using Aura.Api.Services;
using MongoDB.Driver;

namespace Aura.Api.Repositories;

public sealed class UserRepository(MongoContext db) : IUserRepository
{
    public async Task<UserDocument?> FindByEmailAsync(string email) => await db.Users.Find(x => x.Email == email).FirstOrDefaultAsync();
    public async Task<UserDocument?> FindByIdAsync(string id) => await db.Users.Find(x => x.Id == id).FirstOrDefaultAsync();
    public Task<bool> EmailExistsAsync(string email) => db.Users.Find(x => x.Email == email).AnyAsync();
    public Task AddAsync(UserDocument user) => db.Users.InsertOneAsync(user);
    public Task DeleteAsync(string id) => db.Users.DeleteOneAsync(x => x.Id == id);
    public async Task<bool> VerifyAsync(string token)
    {
        var result = await db.Users.UpdateOneAsync(x => x.VerificationToken == token, Builders<UserDocument>.Update.Set(x => x.EmailVerified, true).Set(x => x.VerificationToken, ""));
        return result.ModifiedCount == 1;
    }

    public Task SetPasswordResetTokenAsync(string userId, string tokenHash, DateTime expiresAt) =>
        db.Users.UpdateOneAsync(x => x.Id == userId, Builders<UserDocument>.Update
            .Set(x => x.PasswordResetTokenHash, tokenHash)
            .Set(x => x.PasswordResetTokenExpiresAt, expiresAt));

    public async Task<bool> ResetPasswordAsync(string tokenHash, DateTime now, string passwordHash)
    {
        var result = await db.Users.UpdateOneAsync(
            x => x.PasswordResetTokenHash == tokenHash && x.PasswordResetTokenExpiresAt > now,
            Builders<UserDocument>.Update
                .Set(x => x.PasswordHash, passwordHash)
                .Set(x => x.PasswordResetTokenHash, null)
                .Set(x => x.PasswordResetTokenExpiresAt, null)
                .Inc(x => x.SessionVersion, 1));
        return result.ModifiedCount == 1;
    }

    public Task<bool> IsSessionValidAsync(string userId, int sessionVersion) =>
        db.Users.Find(BuildSessionFilter(userId, sessionVersion)).AnyAsync();

    internal static FilterDefinition<UserDocument> BuildSessionFilter(string userId, int sessionVersion)
    {
        var filters = Builders<UserDocument>.Filter;
        var versionFilter = filters.Eq(user => user.SessionVersion, sessionVersion);

        // SessionVersion was added after accounts already existed. MongoDB does not
        // consider a missing numeric field equal to zero, although deserialization
        // correctly gives those legacy accounts the CLR default value of zero.
        if (sessionVersion == 0)
            versionFilter = filters.Or(
                versionFilter,
                filters.Exists(user => user.SessionVersion, false));

        return filters.And(
            filters.Eq(user => user.Id, userId),
            filters.Ne(user => user.IsBlocked, true),
            versionFilter);
    }
}
