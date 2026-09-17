using Amazon.S3;
using Amazon.S3.Model;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class S3AssetStorage(IAmazonS3 s3, IConfiguration configuration) : IAssetStorage
{
    private readonly string _bucket = !string.IsNullOrWhiteSpace(configuration["AWS:BucketName"])
        ? configuration["AWS:BucketName"]!
        : throw new InvalidOperationException("AWS:BucketName must be configured when S3 is the media storage provider.");

    public Task DeleteAsync(string objectKey) => s3.DeleteObjectAsync(_bucket, objectKey);

    public string ReadUrl(string objectKey) => s3.GetPreSignedURL(new GetPreSignedUrlRequest
    {
        BucketName = _bucket,
        Key = objectKey,
        Expires = DateTime.UtcNow.AddHours(1)
    });
}
