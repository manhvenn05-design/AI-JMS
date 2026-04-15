using System.ComponentModel.DataAnnotations;

namespace AI_JMS.Models.ViewModels;

public class ChangePasswordViewModel
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)] // Bắt buộc tối thiểu 6 ký tự
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}