using Amazon.S3.Model;
using Aura.Api.Common;
using Aura.Api.Models;
using Aura.Api.Repositories;

namespace Aura.Api.Services;

public sealed class UploadService(IMediaRepository media, IUserRepository users, IConfigurationRepository configuration, IMediaStorage storage, IClock clock) : IUploadService
{
    public async Task<UploadResponse> BeginAsync(UploadRequest request, string userId)
    {
        if (!(await configuration.GetAsync()).UploadsEnabled) throw new ServiceException(403, "Uploads are currently unavailable.");
        if (request.Description.Length > 255) throw new ServiceException(400, "Description cannot exceed 255 characters.");
        if (request.FileSize <= 0 || string.IsNullOrWhiteSpace(request.ContentType)) throw new ServiceException(400, "A valid media file is required.");
        var user = await users.FindByIdAsync(userId) ?? throw new ServiceException(401, "User account was not found.");
        var key = $"{user.Id}/{Guid.NewGuid():N}/{Path.GetFileName(request.FileName)}";
        var transfer = await storage.BeginAsync(key, request.ContentType, request.FileSize);
        var item = new MediaDocument { OwnerId = user.Id!, OwnerName = user.Name, Title = string.IsNullOrWhiteSpace(request.Title) ? clock.UtcNow.ToString("yyyy-MM-dd hh mm ss") : request.Title.Trim(), Description = request.Description, Tags = request.Tags?.Select(x => x.Trim().ToLowerInvariant()).Where(x => x.Length > 0).Distinct().ToList() ?? [], MediaType = request.ContentType.Split('/')[0], ObjectKey = key, CreatedAt = clock.UtcNow };
        await media.AddAsync(item);
        return new(item.Id!, transfer.UploadId, transfer.Urls, "Your media is under review.");
    }
    public async Task<MessageResponse> CompleteAsync(string id, CompleteUploadRequest request, string userId)
    {
        var item = await media.FindOwnedAsync(id, userId) ?? throw new ServiceException(404, "Media was not found.");
        await storage.CompleteAsync(item.ObjectKey, request.UploadId, request.Parts.Select(x => new PartETag(x.PartNumber, x.ETag)));
        return new("Your media is under review.");
    }
}
