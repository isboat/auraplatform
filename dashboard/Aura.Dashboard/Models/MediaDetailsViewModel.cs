using Aura.Dashboard.Domain;

namespace Aura.Dashboard.Models;

public sealed record MediaDetailsViewModel(ManagedMedia Media, string PreviewUrl)
{
    public string MediaType => Media.MediaType.Trim().ToLowerInvariant();
}
