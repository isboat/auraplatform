using System.ComponentModel.DataAnnotations;

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
