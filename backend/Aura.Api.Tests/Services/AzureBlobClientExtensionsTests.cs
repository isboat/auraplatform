using Aura.Api.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Aura.Api.Tests.Services;

public sealed class AzureBlobClientExtensionsTests
{
    [Fact]
    public void AuthorizedUri_GeneratesSas_WhenConnectionUsesAccountKey()
    {
        var accountKey = Convert.ToBase64String(new byte[32]);
        var connectionString = $"DefaultEndpointsProtocol=https;AccountName=account;AccountKey={accountKey};EndpointSuffix=core.windows.net";
        var blob = new BlobContainerClient(connectionString, "media").GetBlobClient("asset.mp4");

        var uri = blob.AuthorizedUri(null, BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.Contains("sig=", uri.Query);
        Assert.Contains("sp=r", uri.Query);
    }

    [Fact]
    public void AuthorizedUri_ReusesConfiguredSas_WhenSharedKeyIsUnavailable()
    {
        var blob = new BlobContainerClient(new Uri("https://account.blob.core.windows.net/media"))
            .GetBlobClient("asset.mp4");

        var uri = blob.AuthorizedUri("sv=2025-01-05&sp=rcw&sig=test", BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.Contains("sv=2025-01-05", uri.Query);
        Assert.Contains("sig=test", uri.Query);
    }

    [Fact]
    public void AuthorizedUri_DoesNotDuplicateSasAlreadyPresentInBlobUri()
    {
        const string sas = "sv=2026-02-06&ss=bf&srt=co&sp=rwdlaciytfx&sig=test";
        var blob = new BlobContainerClient(new Uri($"https://account.blob.core.windows.net/media?{sas}"))
            .GetBlobClient("asset.mp4");

        var uri = blob.AuthorizedUri(sas, BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(5));
        var queryParts = uri.Query.TrimStart('?').Split('&');

        Assert.Single(queryParts, part => part.StartsWith("sv=", StringComparison.Ordinal));
        Assert.Single(queryParts, part => part.StartsWith("sig=", StringComparison.Ordinal));
    }

    [Fact]
    public void AuthorizedUri_ExplainsRequiredCredential_WhenNoSasCanBeCreatedOrReused()
    {
        var blob = new BlobContainerClient(new Uri("https://account.blob.core.windows.net/media"))
            .GetBlobClient("asset.mp4");

        var error = Assert.Throws<InvalidOperationException>(() =>
            blob.AuthorizedUri(null, BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(5)));

        Assert.Contains("AccountKey", error.Message);
        Assert.Contains("SharedAccessSignature", error.Message);
    }
}
