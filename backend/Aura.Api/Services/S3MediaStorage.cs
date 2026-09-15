using Amazon.S3;
using Amazon.S3.Model;
using Aura.Api.Models;

namespace Aura.Api.Services;

public sealed class S3MediaStorage(IAmazonS3 s3, IConfiguration configuration) : IMediaStorage
{
    private readonly string _bucket = !string.IsNullOrWhiteSpace(configuration["AWS:BucketName"])
        ? configuration["AWS:BucketName"]!
        : throw new InvalidOperationException("AWS:BucketName must be configured when S3 is the media storage provider.");
    public async Task<(string UploadId, IReadOnlyList<string> Urls)> BeginAsync(string key, string contentType, long size)
    {
        var init = await s3.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest { BucketName = _bucket, Key = key, ContentType = contentType });
        const long partSize = 10 * 1024 * 1024;
        var count = Math.Max(1, (int)Math.Ceiling(size / (double)partSize));
        var urls = Enumerable.Range(1, count).Select(part => s3.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = _bucket, Key = key, UploadId = init.UploadId, PartNumber = part, Verb = HttpVerb.PUT, Expires = DateTime.UtcNow.AddHours(1) })).ToList();
        return (init.UploadId, urls);
    }
    public Task CompleteAsync(string key, string contentType, string uploadId, IReadOnlyList<UploadedPart> parts) => s3.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest { BucketName = _bucket, Key = key, UploadId = uploadId, PartETags = parts.Select(part => new PartETag(part.PartNumber, part.ETag)).ToList() });
    public Task DeleteAsync(string key) => s3.DeleteObjectAsync(_bucket, key);
    public string ReadUrl(string key) => s3.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = _bucket, Key = key, Expires = DateTime.UtcNow.AddHours(1) });
    public Task CheckHealthAsync(CancellationToken cancellationToken) => s3.GetBucketLocationAsync(new GetBucketLocationRequest { BucketName = _bucket }, cancellationToken);
}
