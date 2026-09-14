using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Aura.Api.Tests.Fakes;
using Moq;
namespace Aura.Api.Tests.Services;

public sealed class MediaServiceTests
{
    private readonly Mock<IMediaRepository> _media = new(); private readonly Mock<ICommentRepository> _comments = new(); private readonly Mock<IMediaStorage> _storage = new(); private MediaService Subject() => new(_media.Object, _comments.Object, _storage.Object);
    private void Url(MediaDocument item) { _storage.Setup(x => x.ReadUrl(item.ObjectKey)).Returns("signed-url"); }
    [Fact] public async Task Home_returns_three_ranked_sections_with_ten_item_limits() { var item = TestData.Media(); Url(item); _media.Setup(x => x.LatestApprovedAsync(10)).ReturnsAsync([item]); _media.Setup(x => x.MostViewedAsync(10)).ReturnsAsync([item]); _media.Setup(x => x.MostLikedAsync(10)).ReturnsAsync([item]); var result = await Subject().HomeAsync(); Assert.Single(result.Latest); Assert.Single(result.MostViewed); Assert.Single(result.MostLiked); Assert.Equal("signed-url", result.Latest[0].Url); Assert.Equal(0, result.Latest[0].Comments); }
    [Fact] public async Task Search_uses_fifty_item_limit() { var item = TestData.Media("InReview"); _media.Setup(x => x.SearchAsync("nature", 50)).ReturnsAsync([item]); var result = await Subject().SearchAsync("nature"); Assert.Single(result); Assert.Null(result[0].Url); }
    [Fact] public async Task Get_increments_views_and_includes_comment_count() { var item = TestData.Media(); Url(item); _media.Setup(x => x.FindByIdAsync(item.Id!)).ReturnsAsync(item); _comments.Setup(x => x.CountAsync(item.Id!)).ReturnsAsync(7); var result = await Subject().GetAsync(item.Id!); Assert.Equal(6, result.Views); Assert.Equal(7, result.Comments); _media.Verify(x => x.IncrementViewsAsync(item.Id!), Times.Once); }
    [Fact] public async Task Get_rejects_missing_media() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().GetAsync("missing")); Assert.Equal(404, e.StatusCode); }
    [Fact] public async Task Mine_includes_counts_for_each_upload() { var item = TestData.Media(); Url(item); _media.Setup(x => x.FindByOwnerAsync("user")).ReturnsAsync([item]); _comments.Setup(x => x.CountAsync(item.Id!)).ReturnsAsync(4); var result = await Subject().MineAsync("user"); Assert.Equal(4, Assert.Single(result).Comments); }
    [Fact] public async Task Delete_removes_owned_media_object_and_comments() { var item = TestData.Media(); _media.Setup(x => x.FindOwnedAsync(item.Id!, "user")).ReturnsAsync(item); _media.Setup(x => x.DeleteOwnedAsync(item.Id!, "user")).ReturnsAsync(true); await Subject().DeleteAsync(item.Id!, "user"); _storage.Verify(x => x.DeleteAsync(item.ObjectKey), Times.Once); _comments.Verify(x => x.DeleteForMediaAsync(item.Id!), Times.Once); }
    [Fact] public async Task Delete_rejects_media_not_owned() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().DeleteAsync("missing", "user")); Assert.Equal(404, e.StatusCode); }
    [Fact] public async Task Delete_handles_concurrent_removal() { var item = TestData.Media(); _media.Setup(x => x.FindOwnedAsync(item.Id!, "user")).ReturnsAsync(item); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().DeleteAsync(item.Id!, "user")); Assert.Equal(404, e.StatusCode); }
    [Theory, InlineData(true), InlineData(false)] public async Task React_updates_repository(bool like) { _media.Setup(x => x.SetReactionAsync("media", "user", like)).ReturnsAsync(true); await Subject().ReactAsync("media", "user", like); _media.VerifyAll(); }
    [Fact] public async Task React_rejects_unavailable_media() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().ReactAsync("missing", "user", true)); Assert.Equal(404, e.StatusCode); }
    [Theory, InlineData("Approved"), InlineData("Rejected")] public async Task Review_sets_supported_status(string status) { _media.Setup(x => x.SetReviewStatusAsync("media", status)).ReturnsAsync(true); await Subject().ReviewAsync("media", status); _media.VerifyAll(); }
    [Fact] public async Task Review_rejects_invalid_status() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().ReviewAsync("media", "Pending")); Assert.Equal(400, e.StatusCode); }
    [Fact] public async Task Review_rejects_missing_media() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().ReviewAsync("missing", "Approved")); Assert.Equal(404, e.StatusCode); }
}
