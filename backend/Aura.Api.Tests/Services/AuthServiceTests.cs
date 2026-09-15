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
    private readonly Mock<IConfigurationRepository> _configuration = new();
    private readonly Mock<IPasswordHasher> _passwords = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly Mock<IEmailService> _email = new();
    private AuthService Subject() => new(_users.Object, _configuration.Object, _passwords.Object, _tokens.Object, _email.Object);

    [Fact]
    public async Task Register_creates_normalized_user_and_sends_verification()
    {
        _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration());
        _passwords.Setup(x => x.Hash("secret")).Returns("hashed");
        UserDocument? added = null; _users.Setup(x => x.AddAsync(It.IsAny<UserDocument>())).Callback<UserDocument>(x => { x.Id = "id"; added = x; }).Returns(Task.CompletedTask);
        var result = await Subject().RegisterAsync(new(" Name ", " USER@Example.COM ", "secret"), "https://aura/verify");
        Assert.Equal("Check your email to complete account setup.", result.Message); Assert.Equal("user@example.com", added!.Email); Assert.Equal("Name", added.Name); Assert.Equal("hashed", added.PasswordHash);
        _email.Verify(x => x.SendVerificationAsync("user@example.com", It.Is<string>(url => url.StartsWith("https://aura/verify?token="))), Times.Once);
    }

    [Fact]
    public async Task Register_rejects_disabled_registration()
    { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration { RegistrationEnabled = false }); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().RegisterAsync(new("a", "a@b.com", "x"), "url")); Assert.Equal(403, e.StatusCode); }
    [Theory, InlineData("", "a@b.com", "x"), InlineData("a", "invalid", "x"), InlineData("a", "a@b.com", "")]
    public async Task Register_validates_fields(string name, string email, string password)
    { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().RegisterAsync(new(name, email, password), "url")); Assert.Equal(400, e.StatusCode); }
    [Fact]
    public async Task Register_rejects_duplicate_email()
    { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); _users.Setup(x => x.EmailExistsAsync("a@b.com")).ReturnsAsync(true); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().RegisterAsync(new("a", "a@b.com", "x"), "url")); Assert.Equal(409, e.StatusCode); }
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
}
