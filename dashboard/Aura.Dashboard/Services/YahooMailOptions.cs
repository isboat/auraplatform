namespace Aura.Dashboard.Services;

public sealed class YahooMailOptions
{
    public const string SectionName = "YahooMail";

    public string EmailAddress { get; init; } = string.Empty;
    public string Passkey { get; init; } = string.Empty;
}
