using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Aura.Dashboard.Services;

public sealed class DashboardLinkBuilder(IOptions<DashboardLinkOptions> options)
{
    public string Invitation(string token) => Build("account/accept-invitation", token);

    public string PasswordReset(string token) => Build("account/reset-password", token);

    private string Build(string path, string token)
    {
        var publicUrl = options.Value.PublicUrl;
        if (!Uri.TryCreate(publicUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException("DashboardLinks:PublicUrl must be an absolute HTTP or HTTPS URL.");

        var normalizedBaseUri = new Uri($"{baseUri.AbsoluteUri.TrimEnd('/')}/");
        var url = new Uri(normalizedBaseUri, path).AbsoluteUri;
        return QueryHelpers.AddQueryString(url, "token", token);
    }
}
