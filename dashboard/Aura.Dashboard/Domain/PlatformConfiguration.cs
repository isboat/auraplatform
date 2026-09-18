using MongoDB.Bson.Serialization.Attributes;

namespace Aura.Dashboard.Domain;

public sealed class PlatformConfiguration
{
    [BsonId]
    public string Id { get; set; } = "platform";

    public bool RegistrationEnabled { get; set; } = true;

    public bool UploadsEnabled { get; set; } = true;
}
