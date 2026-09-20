using System.Net;
using Aura.Api.Services;
using Microsoft.AspNetCore.Http;

namespace Aura.Api.Tests.Services;

public sealed class RequestMetadataExtractorTests
{
    [Fact]
    public void Extract_captures_network_client_hints_and_location_headers()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("::ffff:203.0.113.9");
        context.Request.Headers.UserAgent = "Mozilla/5.0 Chrome/140.0.0.0 Safari/537.36";
        context.Request.Headers.AcceptLanguage = "en-GB,en;q=0.9";
        context.Request.Headers["Sec-CH-UA"] = "\"Chromium\";v=\"140\"";
        context.Request.Headers["Sec-CH-UA-Mobile"] = "?1";
        context.Request.Headers["Sec-CH-UA-Platform"] = "\"Android\"";
        context.Request.Headers["CF-IPCountry"] = "GB";

        var result = RequestMetadataExtractor.Extract(context);

        Assert.Equal("203.0.113.9", result.IpAddress);
        Assert.Equal("\"Chromium\";v=\"140\"", result.Browser);
        Assert.Equal("Mobile", result.Device);
        Assert.Equal("Android", result.Platform);
        Assert.Equal("en-GB,en;q=0.9", result.Language);
        Assert.Equal("GB", result.Country);
    }

    [Fact]
    public void Extract_falls_back_to_user_agent_detection_and_sanitizes_values()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0) Firefox/143.0";
        context.Request.Headers["X-Vercel-IP-City"] = "New\nYork";

        var result = RequestMetadataExtractor.Extract(context);

        Assert.Equal("Firefox 143.0", result.Browser);
        Assert.Equal("Desktop", result.Device);
        Assert.Equal("Windows", result.Platform);
        Assert.Equal("New York", result.City);
    }
}
