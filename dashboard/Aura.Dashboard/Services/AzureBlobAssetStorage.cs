using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class AzureBlobAssetStorage : IAssetStorage
{
    private readonly BlobContainerClient _container;
    private readonly string? _configuredSas;

    public AzureBlobAssetStorage(IConfiguration configuration)
    {
        var connectionString = !string.IsNullOrWhiteSpace(configuration["AzureStorage:ConnectionString"])
            ? configuration["AzureStorage:ConnectionString"]!
            : throw new InvalidOperationException(
                "AzureStorage:ConnectionString must be configured when Azure is the media storage provider.");
        var containerName = configuration["AzureStorage:ContainerName"] ?? "aura-media";
        _container = new BlobContainerClient(connectionString, containerName);
        _configuredSas = ConnectionStringValue(connectionString, "SharedAccessSignature");
    }

    public async Task DeleteAsync(string objectKey) =>
        await _container.DeleteBlobIfExistsAsync(objectKey);

    public string ReadUrl(string objectKey) => _container.GetBlobClient(objectKey)
        .AuthorizedUri(_configuredSas, BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1))
        .ToString();

    private static string? ConnectionStringValue(string connectionString, string key) =>
        connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Where(part => part.Length == 2 && part[0].Equals(key, StringComparison.OrdinalIgnoreCase))
            .Select(part => part[1].Trim())
            .FirstOrDefault(value => value.Length > 0);
}

internal static class AzureBlobClientExtensions
{
    public static Uri AuthorizedUri(
        this BlobBaseClient blob,
        string? configuredSas,
        BlobSasPermissions permissions,
        DateTimeOffset expiresAt)
    {
        if (blob.CanGenerateSasUri)
        {
            return blob.GenerateSasUri(permissions, expiresAt);
        }

        if (ContainsSasSignature(blob.Uri.Query))
        {
            return blob.Uri;
        }

        if (string.IsNullOrWhiteSpace(configuredSas))
        {
            throw new InvalidOperationException(
                "Azure Blob read URLs require either an AccountKey or a SharedAccessSignature in AzureStorage:ConnectionString.");
        }

        var builder = new UriBuilder(blob.Uri) { Query = configuredSas.Trim().TrimStart('?') };
        return builder.Uri;
    }

    private static bool ContainsSasSignature(string query) =>
        query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part.StartsWith("sig=", StringComparison.OrdinalIgnoreCase));
}
