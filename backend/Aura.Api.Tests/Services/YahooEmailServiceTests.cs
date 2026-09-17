using Aura.Api.Services;
using Moq;

namespace Aura.Api.Tests.Services;

public sealed class YahooEmailServiceTests
{
    private readonly Mock<IEmailSender> _sender = new();

    [Fact]
    public async Task Verification_email_contains_the_supplied_link()
    {
        var service = new YahooEmailService(_sender.Object);

        await service.SendVerificationAsync("user@example.com", "https://aura.example/verify?token=secret");

        _sender.Verify(sender => sender.SendAsync(
            "user@example.com",
            "Verify your Aura account",
            It.Is<string>(body => body.Contains("https://aura.example/verify?token=secret"))));
    }

    [Fact]
    public async Task Password_reset_email_contains_the_supplied_link()
    {
        var service = new YahooEmailService(_sender.Object);

        await service.SendPasswordResetAsync("user@example.com", "https://aura.example/reset?token=secret");

        _sender.Verify(sender => sender.SendAsync(
            "user@example.com",
            "Reset your Aura password",
            It.Is<string>(body => body.Contains("https://aura.example/reset?token=secret"))));
    }
}
