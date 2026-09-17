using Aura.Dashboard.Controllers;
using Aura.Dashboard.Domain;
using Aura.Dashboard.Models;
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

        var model = Assert.IsType<MediaDetailsViewModel>(result.Model);
        Assert.Same(item, model.Media);
        Assert.Equal("https://signed.example/video.mp4", model.PreviewUrl);
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

    [Theory]
    [InlineData(" Video ", "video")]
    [InlineData("IMAGE", "image")]
    [InlineData("audio", "audio")]
    public void Details_normalizes_the_media_type_for_player_selection(string value, string expected)
    {
        var item = Media();
        item.MediaType = value;

        Assert.Equal(expected, new MediaDetailsViewModel(item, "https://signed.example/asset").MediaType);
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
