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
    public async Task<bool> VerifyAsync(string token)
    {
        var result = await db.Users.UpdateOneAsync(x => x.VerificationToken == token, Builders<UserDocument>.Update.Set(x => x.EmailVerified, true).Set(x => x.VerificationToken, ""));
        return result.ModifiedCount == 1;
    }
}
