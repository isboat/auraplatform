using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Aura.Api.Tests.Fakes;
using Moq;
namespace Aura.Api.Tests.Services;

public sealed class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _comments = new(); private readonly Mock<IUserRepository> _users = new(); private readonly Mock<IMediaRepository> _media = new(); private CommentService Subject() => new(_comments.Object, _users.Object, _media.Object);
    [Theory, InlineData(null, 10), InlineData(0, 1), InlineData(100, 50)] public async Task List_clamps_batch_size(int? requested, int expected) { _comments.Setup(x => x.ListAsync("media", expected, null)).ReturnsAsync([]); await Subject().ListAsync("media", requested, null); _comments.VerifyAll(); }
    [Fact] public async Task Add_trims_and_persists_comment() { var item = TestData.Media(); var user = TestData.User(); _media.Setup(x => x.FindByIdAsync(item.Id!)).ReturnsAsync(item); _users.Setup(x => x.FindByIdAsync("user")).ReturnsAsync(user); CommentDocument? saved = null; _comments.Setup(x => x.AddAsync(It.IsAny<CommentDocument>())).Callback<CommentDocument>(x => saved = x).Returns(Task.CompletedTask); var result = await Subject().AddAsync(item.Id!, new(" hello "), "user"); Assert.Same(saved, result); Assert.Equal("hello", result.Body); Assert.Equal(user.Name, result.UserName); }
    [Fact] public async Task Add_rejects_empty_comment() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().AddAsync("media", new(" "), "user")); Assert.Equal(400, e.StatusCode); }
    [Fact] public async Task Add_rejects_missing_media() { var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().AddAsync("missing", new("hello"), "user")); Assert.Equal(404, e.StatusCode); }
    [Fact] public async Task Add_rejects_missing_user() { _media.Setup(x => x.FindByIdAsync("media")).ReturnsAsync(TestData.Media()); var e = await Assert.ThrowsAsync<ServiceException>(() => Subject().AddAsync("media", new("hello"), "user")); Assert.Equal(401, e.StatusCode); }
}
