using System.Text;
using Aura.Api.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace Aura.Api.Services;

public sealed class AzureBlobMediaStorage : IMediaStorage
{
    private const long BlockSize = 10 * 1024 * 1024;
    private readonly BlobContainerClient _container;
    private readonly string? _configuredSas;

    public AzureBlobMediaStorage(IConfiguration configuration)
    {
        var connectionString = !string.IsNullOrWhiteSpace(configuration["AzureStorage:ConnectionString"])
            ? configuration["AzureStorage:ConnectionString"]!
            : throw new InvalidOperationException("AzureStorage:ConnectionString must be configured when Azure is the media storage provider.");
        var containerName = configuration["AzureStorage:ContainerName"] ?? "aura-media";
        _container = new BlobContainerClient(connectionString, containerName);
        _configuredSas = ConnectionStringValue(connectionString, "SharedAccessSignature");
    }

    public async Task<(string UploadId, IReadOnlyList<string> Urls)> BeginAsync(string key, string contentType, long size)
    {
        await _container.CreateIfNotExistsAsync();
        var uploadId = Guid.NewGuid().ToString("N");
        var blockBlob = _container.GetBlockBlobClient(key);
        var partCount = Math.Max(1, (int)Math.Ceiling(size / (double)BlockSize));
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var urls = Enumerable.Range(1, partCount)
            .Select(partNumber => BuildBlockUploadUri(blockBlob, uploadId, partNumber, expiresAt))
            .ToList();
        return (uploadId, urls);
    }

    public async Task CompleteAsync(string key, string contentType, string uploadId, IReadOnlyList<UploadedPart> parts)
    {
        var blockIds = parts.OrderBy(part => part.PartNumber)
            .Select(part => BlockId(uploadId, part.PartNumber));
        await _container.GetBlockBlobClient(key).CommitBlockListAsync(
            blockIds,
            new CommitBlockListOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } });
    }

    public async Task DeleteAsync(string key) =>
        await _container.DeleteBlobIfExistsAsync(key);

    public string ReadUrl(string key) => _container.GetBlobClient(key)
        .AuthorizedUri(_configuredSas, BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1))
        .ToString();

    public async Task CheckHealthAsync(CancellationToken cancellationToken) =>
        await _container.GetPropertiesAsync(cancellationToken: cancellationToken);

    private string BuildBlockUploadUri(
        BlockBlobClient blob,
        string uploadId,
        int partNumber,
        DateTimeOffset expiresAt)
    {
        var sasUri = blob.AuthorizedUri(_configuredSas, BlobSasPermissions.Write | BlobSasPermissions.Create, expiresAt);
        var builder = new UriBuilder(sasUri);
        var blockQuery = $"comp=block&blockid={Uri.EscapeDataString(BlockId(uploadId, partNumber))}";
        builder.Query = string.IsNullOrEmpty(builder.Query)
            ? blockQuery
            : $"{builder.Query.TrimStart('?')}&{blockQuery}";
        return builder.Uri.ToString();
    }

    private static string BlockId(string uploadId, int partNumber) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{uploadId}:{partNumber:D6}"));

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

        var sas = !string.IsNullOrWhiteSpace(configuredSas)
            ? configuredSas
            : blob.Uri.Query;
        if (string.IsNullOrWhiteSpace(sas))
        {
            throw new InvalidOperationException(
                "Azure Blob upload URLs require either an AccountKey or a SharedAccessSignature in AzureStorage:ConnectionString.");
        }

        var builder = new UriBuilder(blob.Uri);
        builder.Query = MergeQuery(builder.Query, sas);
        return builder.Uri;
    }

    private static string MergeQuery(string first, string second)
    {
        var values = new[] { first, second }
            .Select(value => value.Trim().TrimStart('?'))
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.Ordinal);
        return string.Join('&', values);
    }
}
