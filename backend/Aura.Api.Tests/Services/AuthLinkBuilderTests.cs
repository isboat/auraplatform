using Aura.Api.Services;
using Microsoft.Extensions.Options;

namespace Aura.Api.Tests.Services;

public sealed class AuthLinkBuilderTests
{
    [Fact]
    public void Links_use_configured_origins_instead_of_request_headers()
    {
        var links = new AuthLinkBuilder(Options.Create(new AuthLinkOptions
        {
            ApiPublicUrl = "https://api.aura.example/",
            FrontendPublicUrl = "https://aura.example/"
        }));

        Assert.Equal("https://api.aura.example/api/auth/verify", links.VerificationBaseUrl);
        Assert.Equal("https://aura.example/reset-password", links.PasswordResetBaseUrl);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://api.aura.example")]
    [InlineData("https://api.aura.example?redirect=evil")]
    public void Verification_origin_must_be_a_safe_absolute_http_url(string url)
    {
        var links = new AuthLinkBuilder(Options.Create(new AuthLinkOptions { ApiPublicUrl = url }));

        Assert.Throws<InvalidOperationException>(() => links.VerificationBaseUrl);
    }
}
