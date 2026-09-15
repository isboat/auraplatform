using System.ComponentModel.DataAnnotations;

namespace Aura.Dashboard.Services;

public sealed class CreateStaffUserRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Role { get; init; } = string.Empty;
}
