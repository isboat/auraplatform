using Amazon.S3;
using Amazon.S3.Model;
using Aura.Dashboard.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Aura.Dashboard.Tests;

public sealed class AssetStorageTests
{
    [Fact]
    public void S3_read_url_uses_the_configured_bucket_and_object_key()
    {
        var client = new Mock<IAmazonS3>();
        GetPreSignedUrlRequest? request = null;
        client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(value => request = value)
            .Returns("https://signed.example/asset");
        var storage = new S3AssetStorage(client.Object, Configuration(("AWS:BucketName", "media")));

        var url = storage.ReadUrl("owner/asset.mp4");

        Assert.Equal("https://signed.example/asset", url);
        Assert.Equal("media", request!.BucketName);
        Assert.Equal("owner/asset.mp4", request.Key);
        Assert.InRange(request.Expires, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
    }

    [Fact]
    public async Task S3_delete_removes_the_object_from_the_configured_bucket()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(x => x.DeleteObjectAsync("media", "owner/asset.mp4", default))
            .ReturnsAsync(new DeleteObjectResponse());
        var storage = new S3AssetStorage(client.Object, Configuration(("AWS:BucketName", "media")));

        await storage.DeleteAsync("owner/asset.mp4");

        client.Verify(x => x.DeleteObjectAsync("media", "owner/asset.mp4", default));
    }

    [Fact]
    public void Azure_read_url_generates_a_read_only_sas()
    {
        var accountKey = Convert.ToBase64String(new byte[32]);
        var connectionString = $"DefaultEndpointsProtocol=https;AccountName=account;AccountKey={accountKey};EndpointSuffix=core.windows.net";
        var storage = new AzureBlobAssetStorage(Configuration(
            ("AzureStorage:ConnectionString", connectionString),
            ("AzureStorage:ContainerName", "media")));

        var uri = new Uri(storage.ReadUrl("owner/asset.mp4"));

        Assert.Equal("/media/owner/asset.mp4", uri.AbsolutePath);
        Assert.Contains("sig=", uri.Query);
        Assert.Contains("sp=r", uri.Query);
    }

    [Fact]
    public void Azure_read_url_reuses_a_connection_string_sas_without_duplicating_it()
    {
        const string sas = "sv=2026-02-06&ss=bf&srt=co&sp=r&sig=test";
        var storage = new AzureBlobAssetStorage(Configuration(
            ("AzureStorage:ConnectionString", $"BlobEndpoint=https://account.blob.core.windows.net/;SharedAccessSignature={sas}"),
            ("AzureStorage:ContainerName", "media")));

        var queryParts = new Uri(storage.ReadUrl("asset.mp4")).Query.TrimStart('?').Split('&');

        Assert.Single(queryParts, part => part.StartsWith("sv=", StringComparison.Ordinal));
        Assert.Single(queryParts, part => part.StartsWith("sig=", StringComparison.Ordinal));
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => (string?)x.Value))
            .Build();
}
