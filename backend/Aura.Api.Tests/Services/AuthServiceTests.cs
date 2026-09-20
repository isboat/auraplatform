using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Aura.Api.Tests.Fakes;
using Moq;

namespace Aura.Api.Tests.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IConfigurationService> _configuration = new();
    private readonly Mock<IPasswordHasher> _passwords = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<IClock> _clock = new();
    private AuthService Subject() => new(_users.Object, _configuration.Object, _passwords.Object, _tokens.Object, _email.Object, _clock.Object);

    [Fact]
    public async Task Register_creates_normalized_user_and_sends_verification()
    {
        var registeredAt = new DateTime(2026, 9, 20, 14, 30, 0, DateTimeKind.Utc);
        _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration());
        _passwords.Setup(x => x.Hash("secret")).Returns("hashed");
        _clock.SetupGet(x => x.UtcNow).Returns(registeredAt);
        var metadata = new ClientMetadata { IpAddress = "203.0.113.10", Browser = "Firefox 143.0", Device = "Desktop", Country = "GB" };
        UserDocument? added = null; _users.Setup(x => x.AddAsync(It.IsAny<UserDocument>())).Callback<UserDocument>(x => { x.Id = "id"; added = x; }).Returns(Task.CompletedTask);
        var result = await Subject().RegisterAsync(new(" Name ", " USER@Example.COM ", "secret", " +1 202 555 0147 "), "https://aura/verify", metadata);
        Assert.Equal("Check your email to complete account setup.", result.Message); Assert.Equal("user@example.com", added!.Email); Assert.Equal("Name", added.Name); Assert.Equal("hashed", added.PasswordHash);
        Assert.Equal(registeredAt, added.CreatedAt);
        Assert.Equal("+1 202 555 0147", added.Phone);
        Assert.Same(metadata, added.RegistrationMetadata);
        _email.Verify(x => x.SendVerificationAsync("user@example.com", It.Is<string>(url => url.StartsWith("https://aura/verify?token="))), Times.Once);
    }

    [Fact]
    public async Task Register_removes_the_new_user_when_verification_delivery_fails()
    {
        _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration());
        _users.Setup(x => x.AddAsync(It.IsAny<UserDocument>()))
            .Callback<UserDocument>(user => user.Id = "new-user")
            .Returns(Task.CompletedTask);
        _email.Setup(x => x.SendVerificationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Subject().RegisterAsync(new("Name", "user@example.com", "secret"), "https://api.aura.example/api/auth/verify", new ClientMetadata()));

        _users.Verify(x => x.DeleteAsync("new-user"), Times.Once);
    }

    [Fact]
    public async Task Register_rejects_disabled_registration()
    { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration { RegistrationEnabled = false }); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().RegisterAsync(new("a", "a@b.com", "x"), "url", new ClientMetadata())); Assert.Equal(403, e.StatusCode); }
    [Theory, InlineData("", "a@b.com", "x"), InlineData("a", "invalid", "x"), InlineData("a", "a@b.com", "")]
    public async Task Register_validates_fields(string name, string email, string password)
    { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().RegisterAsync(new(name, email, password), "url", new ClientMetadata())); Assert.Equal(400, e.StatusCode); }
    [Fact]
    public async Task Register_rejects_duplicate_email()
    { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); _users.Setup(x => x.EmailExistsAsync("a@b.com")).ReturnsAsync(true); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().RegisterAsync(new("a", "a@b.com", "x"), "url", new ClientMetadata())); Assert.Equal(409, e.StatusCode); }
    [Fact]
    public async Task Verify_returns_confirmation()
    { _users.Setup(x => x.VerifyAsync("token")).ReturnsAsync(true); Assert.Equal("Your account is verified.", (await Subject().VerifyAsync("token")).Message); }
    [Fact]
    public async Task Verify_rejects_bad_token()
    { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().VerifyAsync("bad")); Assert.Equal(404, e.StatusCode); }
    [Fact]
    public async Task Login_returns_token_for_verified_user()
    { var user = TestData.User(); _users.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user); _passwords.Setup(x => x.Verify("secret", "hash")).Returns(true); _tokens.Setup(x => x.Create(user)).Returns("jwt"); var result = await Subject().LoginAsync(new(" USER@EXAMPLE.COM ", "secret")); Assert.Equal("jwt", result.Token); Assert.Equal(user.Name, result.User.Name); }
    [Fact]
    public async Task Login_rejects_missing_or_bad_password()
    { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().LoginAsync(new("missing@example.com", "bad"))); Assert.Equal(401, e.StatusCode); }
    [Fact]
    public async Task Login_rejects_unverified_user()
    { var user = TestData.User(false); _users.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user); _passwords.Setup(x => x.Verify("secret", "hash")).Returns(true); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().LoginAsync(new(user.Email, "secret"))); Assert.Equal(403, e.StatusCode); }
    [Fact]
    public async Task Login_rejects_blocked_user_with_customer_service_contact()
    { var user = TestData.User(); user.IsBlocked = true; _users.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user); _passwords.Setup(x => x.Verify("secret", "hash")).Returns(true); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().LoginAsync(new(user.Email, "secret"))); Assert.Equal(403, e.StatusCode); Assert.Contains("support@auraplatform.com", e.Message); _tokens.Verify(x => x.Create(It.IsAny<UserDocument>()), Times.Never); }

    [Fact]
    public async Task Forgot_password_stores_expiring_hashed_token_and_sends_link()
    {
        var now = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);
        var user = TestData.User();
        _clock.SetupGet(x => x.UtcNow).Returns(now);
        _users.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);

        var result = await Subject().ForgotPasswordAsync(new(" USER@EXAMPLE.COM "), "https://aura/reset-password");

        Assert.Contains("If an account exists", result.Message);
        _users.Verify(x => x.SetPasswordResetTokenAsync(user.Id!, It.Is<string>(hash => hash.Length == 64), now.AddHours(1)), Times.Once);
        _email.Verify(x => x.SendPasswordResetAsync(user.Email, It.Is<string>(url => url.StartsWith("https://aura/reset-password?token=") && url.Length > 70)), Times.Once);
    }

    [Fact]
    public async Task Forgot_password_does_not_reveal_unknown_or_unverified_email()
    {
        var unknown = await Subject().ForgotPasswordAsync(new("missing@example.com"), "https://aura/reset-password");
        _users.Setup(x => x.FindByEmailAsync("user@example.com")).ReturnsAsync(TestData.User(false));
        var unverified = await Subject().ForgotPasswordAsync(new("user@example.com"), "https://aura/reset-password");

        Assert.Equal(unknown.Message, unverified.Message);
        _email.Verify(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Reset_password_hashes_password_and_consumes_token()
    {
        var now = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);
        _clock.SetupGet(x => x.UtcNow).Returns(now);
        _passwords.Setup(x => x.Hash("new-password")).Returns("new-hash");
        _users.Setup(x => x.ResetPasswordAsync(It.Is<string>(hash => hash.Length == 64), now, "new-hash")).ReturnsAsync(true);

        var result = await Subject().ResetPasswordAsync(new("one-time-token", "new-password", "new-password"));

        Assert.Equal("Your password has been reset. Sign in with your new password.", result.Message);
    }

    [Theory]
    [InlineData("token", "short", "short")]
    [InlineData("token", "password-one", "password-two")]
    [InlineData("", "new-password", "new-password")]
    public async Task Reset_password_rejects_invalid_input(string token, string password, string confirmation)
    {
        var error = await Assert.ThrowsAsync<ServiceException>(() => Subject().ResetPasswordAsync(new(token, password, confirmation)));
        Assert.Equal(400, error.StatusCode);
    }
}
