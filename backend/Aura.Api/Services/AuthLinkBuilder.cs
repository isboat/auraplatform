using Microsoft.Extensions.Options;

namespace Aura.Api.Services;

public sealed class AuthLinkBuilder(IOptions<AuthLinkOptions> options)
{
    public string VerificationBaseUrl => Build(options.Value.ApiPublicUrl, "api/auth/verify", "AuthLinks:ApiPublicUrl");

    public string PasswordResetBaseUrl => Build(options.Value.FrontendPublicUrl, "reset-password", "AuthLinks:FrontendPublicUrl");

    private static string Build(string publicUrl, string path, string settingName)
    {
        if (!Uri.TryCreate(publicUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(baseUri.Query) || !string.IsNullOrEmpty(baseUri.Fragment))
            throw new InvalidOperationException($"{settingName} must be an absolute HTTP or HTTPS URL without a query or fragment.");

        var normalizedBaseUri = new Uri($"{baseUri.AbsoluteUri.TrimEnd('/')}/");
        return new Uri(normalizedBaseUri, path).AbsoluteUri;
    }
}
