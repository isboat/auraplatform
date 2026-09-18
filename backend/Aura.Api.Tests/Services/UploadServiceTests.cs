using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Aura.Api.Tests.Fakes;
using Moq;

namespace Aura.Api.Tests.Services;

public sealed class UploadServiceTests
{
    private readonly Mock<IMediaRepository> _media = new(); private readonly Mock<IUserRepository> _users = new(); private readonly Mock<IConfigurationService> _configuration = new(); private readonly Mock<IMediaStorage> _storage = new(); private readonly Mock<IClock> _clock = new();
    private UploadService Subject() => new(_media.Object, _users.Object, _configuration.Object, _storage.Object, _clock.Object);
    private void ValidSetup()
    {
        _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); _users.Setup(x => x.FindByIdAsync("user")).ReturnsAsync(TestData.User()); _storage.Setup(x => x.BeginAsync(It.IsAny<string>(), "video/mp4", 100)).ReturnsAsync(("upload", (IReadOnlyList<string>)["part"])); _clock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 9, 14, 9, 8, 7, DateTimeKind.Utc)); _media.Setup(x => x.AddAsync(It.IsAny<MediaDocument>())).Callback<MediaDocument>(x => x.Id = "media").Returns(Task.CompletedTask);
    }
    [Fact]
    public async Task Begin_creates_normalized_media_and_default_title()
    { ValidSetup(); MediaDocument? added = null; _media.Setup(x => x.AddAsync(It.IsAny<MediaDocument>())).Callback<MediaDocument>(x => { x.Id = "media"; added = x; }).Returns(Task.CompletedTask); var result = await Subject().BeginAsync(new(null, "description", [" Nature ", "nature", " Film "], "../clip.mp4", "video/mp4", 100), "user"); Assert.Equal("media", result.MediaId); Assert.Equal("upload", result.UploadId); Assert.Equal("2026-09-14 09 08 07", added!.Title); Assert.Equal(["nature", "film"], added.Tags); Assert.EndsWith("/clip.mp4", added.ObjectKey); Assert.Equal("Your media is under review.", result.Message); }
    [Fact]
    public async Task Begin_preserves_trimmed_custom_title_and_empty_tags()
    { ValidSetup(); MediaDocument? added = null; _media.Setup(x => x.AddAsync(It.IsAny<MediaDocument>())).Callback<MediaDocument>(x => { x.Id = "media"; added = x; }).Returns(Task.CompletedTask); await Subject().BeginAsync(new(" Custom ", "", null, "clip.mp4", "video/mp4", 100), "user"); Assert.Equal("Custom", added!.Title); Assert.Empty(added.Tags); }
    [Fact] public async Task Begin_rejects_disabled_uploads() { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration { UploadsEnabled = false }); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().BeginAsync(new(null, "", null, "x", "video/mp4", 1), "user")); Assert.Equal(403, e.StatusCode); }
    [Fact] public async Task Begin_rejects_long_description() { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().BeginAsync(new(null, new string('x', 256), null, "x", "video/mp4", 1), "user")); Assert.Equal(400, e.StatusCode); }
    [Theory, InlineData(0, "video/mp4"), InlineData(1, "")]
    public async Task Begin_rejects_invalid_file(long size, string contentType) { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().BeginAsync(new(null, "", null, "x", contentType, size), "user")); Assert.Equal(400, e.StatusCode); }
    [Fact] public async Task Begin_rejects_unknown_user() { _configuration.Setup(x => x.GetAsync()).ReturnsAsync(new PlatformConfiguration()); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().BeginAsync(new(null, "", null, "x", "video/mp4", 1), "user")); Assert.Equal(401, e.StatusCode); }
    [Fact] public async Task Complete_finishes_owned_multipart_upload() { var item = TestData.Media(); _media.Setup(x => x.FindOwnedAsync(item.Id!, "user")).ReturnsAsync(item); var result = await Subject().CompleteAsync(item.Id!, new("upload", [new(1, "etag")]), "user"); Assert.Equal("Your media is under review.", result.Message); _storage.Verify(x => x.CompleteAsync(item.ObjectKey, item.ContentType, "upload", It.Is<IReadOnlyList<UploadedPart>>(parts => parts.Single().PartNumber == 1)), Times.Once); }
    [Fact] public async Task Complete_rejects_unknown_media() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().CompleteAsync("missing", new("upload", []), "user")); Assert.Equal(404, e.StatusCode); }
}
