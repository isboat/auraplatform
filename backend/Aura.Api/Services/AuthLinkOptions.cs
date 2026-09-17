namespace Aura.Api.Services;

public sealed class AuthLinkOptions
{
    public const string SectionName = "AuthLinks";

    public string ApiPublicUrl { get; init; } = "http://localhost:5080";
    public string FrontendPublicUrl { get; init; } = "http://localhost:5173";
}
