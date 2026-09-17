using System.ComponentModel.DataAnnotations;

namespace Aura.Dashboard.Models;

public sealed class ResetPasswordViewModel
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;
}
