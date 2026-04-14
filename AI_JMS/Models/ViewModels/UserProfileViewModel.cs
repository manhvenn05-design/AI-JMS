namespace AI_JMS.Models.ViewModels; // Lưu ý đường dẫn có .Models.

public class UserProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Avatar { get; set; }
    public string? AcademicTitle { get; set; }
    public string? Affiliation { get; set; }
    public string? ExpertiseKeywords { get; set; }
}