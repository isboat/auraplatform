using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Moq;
using Xunit;

namespace Aura.Dashboard.Tests;

public sealed class StaffCredentialServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ISecureTokenService> _tokens = new();

    [Fact]
    public async Task Invitation_is_consumed_with_a_hashed_token_and_password()
    {
        _tokens.Setup(x => x.Hash("invitation-token")).Returns("token-hash");
        _users.Setup(x => x.AcceptInvitationAsync("token-hash", It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(true);
        var service = new StaffCredentialService(_users.Object, _tokens.Object);

        Assert.True(await service.AcceptInvitationAsync("invitation-token", "Strong!Password1"));

        _users.Verify(x => x.AcceptInvitationAsync(
            "token-hash",
            It.IsAny<DateTime>(),
            It.Is<string>(hash => BCrypt.Net.BCrypt.Verify("Strong!Password1", hash))));
    }

    [Fact]
    public async Task Password_reset_is_consumed_with_a_hashed_token_and_password()
    {
        _tokens.Setup(x => x.Hash("reset-token")).Returns("token-hash");
        _users.Setup(x => x.ResetPasswordAsync("token-hash", It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(true);
        var service = new StaffCredentialService(_users.Object, _tokens.Object);

        Assert.True(await service.ResetPasswordAsync("reset-token", "Strong!Password1"));

        _users.Verify(x => x.ResetPasswordAsync(
            "token-hash",
            It.IsAny<DateTime>(),
            It.Is<string>(hash => BCrypt.Net.BCrypt.Verify("Strong!Password1", hash))));
    }
}
