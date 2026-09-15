using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class LoggingResetDelivery(ILogger<LoggingResetDelivery> logger):IResetDelivery { public Task SendAsync(StaffUser user,string token,DateTime expiresAtUtc){logger.LogInformation("Password reset delivery requested for user {UserId}; expires {ExpiresAtUtc}",user.Id,expiresAtUtc);return Task.CompletedTask;} }
