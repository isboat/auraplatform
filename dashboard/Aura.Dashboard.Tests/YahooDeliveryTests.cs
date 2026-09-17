using Aura.Dashboard.Domain;
using Aura.Dashboard.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Aura.Dashboard.Tests;

public sealed class YahooDeliveryTests
{
    private readonly Mock<IEmailSender> _sender = new();
    private readonly DashboardLinkBuilder _links = new(Options.Create(new DashboardLinkOptions
    {
        PublicUrl = "https://dashboard.aura.example/"
    }));

    [Fact]
    public async Task Invitation_contains_an_encoded_token_and_expiration()
    {
        var delivery = new YahooStaffInvitationDelivery(_sender.Object, _links);
        var expiration = new DateTime(2026, 9, 18, 10, 30, 0, DateTimeKind.Utc);

        await delivery.SendAsync(User(), "secret token/+", expiration);

        _sender.Verify(sender => sender.SendAsync(
            "reviewer@example.com",
            "You are invited to Aura",
            It.Is<string>(body => body.Contains("account/accept-invitation?token=secret%20token%2F%2B") &&
                                  body.Contains("2026-09-18 10:30:00Z"))));
    }

    [Fact]
    public async Task Password_reset_contains_an_encoded_token_and_expiration()
    {
        var delivery = new YahooResetDelivery(_sender.Object, _links);
        var expiration = new DateTime(2026, 9, 18, 10, 30, 0, DateTimeKind.Utc);

        await delivery.SendAsync(User(), "secret token/+", expiration);

        _sender.Verify(sender => sender.SendAsync(
            "reviewer@example.com",
            "Reset your Aura password",
            It.Is<string>(body => body.Contains("account/reset-password?token=secret%20token%2F%2B") &&
                                  body.Contains("2026-09-18 10:30:00Z"))));
    }

    private static StaffUser User() => new() { Name = "Rita", Email = "reviewer@example.com" };
}
