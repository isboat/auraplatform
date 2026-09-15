using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class LoggingResetDelivery(ILogger<LoggingResetDelivery> logger):IResetDelivery { public Task SendAsync(StaffUser user){logger.LogInformation("Password reset requested for user {UserId}",user.Id);return Task.CompletedTask;} }
