using Aura.Api.Models;
using Aura.Api.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Aura.Api.Tests.Repositories;

public sealed class UserRepositoryTests
{
    [Fact]
    public void Legacy_zero_version_session_accepts_missing_session_version()
    {
        var filter = UserRepository.BuildSessionFilter("507f1f77bcf86cd799439011", 0);

        var rendered = filter.Render(new RenderArgs<UserDocument>(
            BsonSerializer.SerializerRegistry.GetSerializer<UserDocument>(),
            BsonSerializer.SerializerRegistry));

        Assert.Contains("$exists", rendered.ToJson());
        Assert.Contains("SessionVersion", rendered.ToJson());
    }

    [Fact]
    public void Versioned_session_requires_exact_session_version()
    {
        var filter = UserRepository.BuildSessionFilter("507f1f77bcf86cd799439011", 2);

        var rendered = filter.Render(new RenderArgs<UserDocument>(
            BsonSerializer.SerializerRegistry.GetSerializer<UserDocument>(),
            BsonSerializer.SerializerRegistry));

        Assert.DoesNotContain("$exists", rendered.ToJson());
        Assert.Contains("2", rendered.ToJson());
    }
}
