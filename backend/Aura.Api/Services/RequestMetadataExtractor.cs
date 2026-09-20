using System.Net;
using System.Text.RegularExpressions;
using Aura.Api.Models;

namespace Aura.Api.Services;

public static partial class RequestMetadataExtractor
{
    private const int MaximumValueLength = 512;

    public static UploadMetadata Extract(HttpContext context)
    {
        var headers = context.Request.Headers;
        var userAgent = Value(headers.UserAgent);
        var platform = Value(headers["Sec-CH-UA-Platform"])?.Trim('"') ?? DetectPlatform(userAgent);
        var mobileHint = Value(headers["Sec-CH-UA-Mobile"]);

        return new UploadMetadata
        {
            IpAddress = NormalizeAddress(context.Connection.RemoteIpAddress),
            Browser = Value(headers["Sec-CH-UA"]) ?? DetectBrowser(userAgent),
            Device = DetectDevice(userAgent, mobileHint),
            Platform = platform,
            UserAgent = userAgent,
            Language = Value(headers.AcceptLanguage),
            Country = First(headers, "CF-IPCountry", "CloudFront-Viewer-Country", "X-Vercel-IP-Country"),
            Region = First(headers, "X-Vercel-IP-Country-Region"),
            City = First(headers, "X-Vercel-IP-City"),
            TimeZone = First(headers, "X-Vercel-IP-Timezone")
        };
    }

    private static string? NormalizeAddress(IPAddress? address)
    {
        if (address is null) return null;
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4().ToString() : address.ToString();
    }

    private static string? First(IHeaderDictionary headers, params string[] names)
    {
        foreach (var name in names)
        {
            var value = Value(headers[name]);
            if (value is not null) return value;
        }
        return null;
    }

    private static string? Value(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var sanitized = ControlCharacters().Replace(value, " ").Trim();
        return sanitized[..Math.Min(sanitized.Length, MaximumValueLength)];
    }

    private static string? DetectBrowser(string? userAgent)
    {
        if (userAgent is null) return null;
        foreach (var (token, name) in new[] { ("Edg/", "Edge"), ("OPR/", "Opera"), ("Chrome/", "Chrome"), ("Firefox/", "Firefox"), ("Version/", "Safari") })
        {
            var index = userAgent.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (index < 0) continue;
            var version = userAgent[(index + token.Length)..].Split(' ', ';', ')')[0];
            return $"{name} {version}";
        }
        return "Other";
    }

    private static string? DetectPlatform(string? userAgent)
    {
        if (userAgent is null) return null;
        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android";
        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iOS";
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows";
        if (userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase)) return "macOS";
        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux";
        return "Other";
    }

    private static string? DetectDevice(string? userAgent, string? mobileHint)
    {
        if (mobileHint == "?1") return "Mobile";
        if (userAgent is null && mobileHint is null) return null;
        if (userAgent?.Contains("iPad", StringComparison.OrdinalIgnoreCase) == true || userAgent?.Contains("Tablet", StringComparison.OrdinalIgnoreCase) == true) return "Tablet";
        if (userAgent?.Contains("Mobile", StringComparison.OrdinalIgnoreCase) == true || userAgent?.Contains("iPhone", StringComparison.OrdinalIgnoreCase) == true) return "Mobile";
        if (userAgent?.Contains("bot", StringComparison.OrdinalIgnoreCase) == true || userAgent?.Contains("crawler", StringComparison.OrdinalIgnoreCase) == true) return "Bot";
        return "Desktop";
    }

    [GeneratedRegex(@"[\u0000-\u001F\u007F]")]
    private static partial Regex ControlCharacters();
}
