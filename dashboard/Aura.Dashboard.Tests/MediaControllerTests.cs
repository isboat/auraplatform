using Aura.Dashboard.Controllers;
using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Aura.Dashboard.Tests;

public sealed class MediaControllerTests
{
    [Fact]
    public async Task Details_provides_a_signed_preview_url()
    {
        var item = Media();
        var media = new Mock<IMediaRepository>();
        media.Setup(x => x.FindAsync("media-id")).ReturnsAsync(item);
        var storage = new Mock<IAssetStorage>();
        storage.Setup(x => x.ReadUrl(item.ObjectKey)).Returns("https://signed.example/video.mp4");
        var controller = new MediaController(media.Object, Mock.Of<IMediaManagementService>(), storage.Object);

        var result = Assert.IsType<ViewResult>(await controller.Details("media-id"));

        Assert.Same(item, result.Model);
        Assert.Equal("https://signed.example/video.mp4", controller.ViewBag.PreviewUrl);
    }

    [Fact]
    public async Task Details_returns_not_found_for_missing_media()
    {
        var controller = new MediaController(
            Mock.Of<IMediaRepository>(),
            Mock.Of<IMediaManagementService>(),
            Mock.Of<IAssetStorage>());

        Assert.IsType<NotFoundResult>(await controller.Details("missing"));
    }

    private static ManagedMedia Media() => new()
    {
        Id = "media-id",
        OwnerId = "owner-id",
        OwnerName = "Aura User",
        Title = "A video",
        MediaType = "video",
        ObjectKey = "owner/video.mp4"
    };
}
