using Amazon.S3;
using Amazon.S3.Model;

namespace Aura.Api.Services;

public sealed class MediaStorage(IAmazonS3 s3, IConfiguration configuration)
{
    private readonly string _bucket = configuration["AWS:BucketName"]!;
    public async Task<(string uploadId, IReadOnlyList<string> urls)> BeginAsync(string key, string contentType, long size)
    {
        var init = await s3.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest { BucketName = _bucket, Key = key, ContentType = contentType });
        const long partSize = 10 * 1024 * 1024;
        var count = Math.Max(1, (int)Math.Ceiling(size / (double)partSize));
        var urls = Enumerable.Range(1, count).Select(part => s3.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = _bucket, Key = key, UploadId = init.UploadId, PartNumber = part, Verb = HttpVerb.PUT, Expires = DateTime.UtcNow.AddHours(1) })).ToList();
        return (init.UploadId, urls);
    }
    public Task CompleteAsync(string key, string uploadId, IEnumerable<PartETag> parts) => s3.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest { BucketName = _bucket, Key = key, UploadId = uploadId, PartETags = parts.ToList() });
    public Task DeleteAsync(string key) => s3.DeleteObjectAsync(_bucket, key);
    public string ReadUrl(string key) => s3.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = _bucket, Key = key, Expires = DateTime.UtcNow.AddHours(1) });
}
