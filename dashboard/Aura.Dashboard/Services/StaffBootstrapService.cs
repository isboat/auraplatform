using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public sealed class FirstAdministratorRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Required, Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string SetupToken { get; init; } = string.Empty;
}

public interface IStaffBootstrapService
{
    Task<bool> IsAvailableAsync();
    Task<StaffUser> CreateFirstAdministratorAsync(FirstAdministratorRequest request, string correlationId);
}

public sealed class StaffBootstrapService(
    IUserRepository users,
    IAuditRepository audit,
    IConfiguration configuration) : IStaffBootstrapService
{
    public async Task<bool> IsAvailableAsync() =>
        HasConfiguredToken() && !await users.HasAdministratorAsync();

    public async Task<StaffUser> CreateFirstAdministratorAsync(
        FirstAdministratorRequest request,
        string correlationId)
    {
        if (await users.HasAdministratorAsync())
            throw new DashboardRuleException("The first administrator has already been created.");

        ValidateSetupToken(request.SetupToken);
        ValidatePassword(request.Password);
        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            throw new DashboardRuleException("The password and confirmation do not match.");

        if (!await users.TryAcquireFirstAdministratorBootstrapAsync())
            throw new DashboardRuleException("Administrator setup is already complete or in progress.");

        try
        {
            // Recheck after acquiring the distributed MongoDB lock. This prevents two
            // dashboard instances from creating competing first administrators.
            if (await users.HasAdministratorAsync())
                throw new DashboardRuleException("The first administrator has already been created.");

            var email = request.Email.Trim().ToLowerInvariant();
            if (await users.FindByEmailAsync(email) is not null)
                throw new DashboardRuleException("An account with this email address already exists.");

            var administrator = new StaffUser
            {
                Name = request.Name.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                EmailVerified = true,
                IsAdministrator = true,
                Roles = [Roles.Administrator]
            };

            await users.SaveAsync(administrator);
            await audit.AppendAsync(new AuditEvent
            {
                EventType = "FirstAdministratorCreated",
                ActorId = administrator.Id ?? email,
                ActorName = administrator.Name,
                ActorEmail = administrator.Email,
                ActorRole = Roles.Administrator,
                TargetType = "StaffUser",
                TargetId = administrator.Id ?? email,
                TargetDisplay = administrator.Email,
                CorrelationId = correlationId
            });

            return administrator;
        }
        finally
        {
            // The administrator record is the permanent completion marker. The lock
            // only serializes concurrent setup requests and is released for retries.
            await users.ReleaseFirstAdministratorBootstrapAsync();
        }
    }

    private bool HasConfiguredToken() =>
        !string.IsNullOrWhiteSpace(configuration["Bootstrap:Token"]);

    private void ValidateSetupToken(string suppliedToken)
    {
        var configuredToken = configuration["Bootstrap:Token"];
        if (string.IsNullOrWhiteSpace(configuredToken) || !FixedTimeEquals(configuredToken, suppliedToken))
            throw new DashboardRuleException("The setup token is invalid.");
    }

    private static bool FixedTimeEquals(string expected, string supplied)
    {
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        return CryptographicOperations.FixedTimeEquals(expectedHash, suppliedHash);
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 12 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) ||
            !password.Any(character => !char.IsLetterOrDigit(character)))
            throw new DashboardRuleException(
                "Use at least 12 characters with uppercase, lowercase, a number, and a symbol.");
    }
}
