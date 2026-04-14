using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_JMS.Models;

public class tblUsers
{
    // --- 1. Thông tin định danh (Identity) ---
    [Key]
    public int UserId { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? Avatar { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }

    // --- 2. Cấu hình hệ thống (Settings & Status) ---
    [StringLength(10)]
    public string PreferredLanguage { get; set; } = "vi";

    public int? LastSelectedRoleId { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // --- 3. Các quan hệ (Navigation Properties) ---
    
    // Gán vai trò (Author, Editor...)
    public virtual ICollection<tblUserRoles> UserRoles { get; set; } = new List<tblUserRoles>();

    // Hồ sơ khoa học (1-1)
    // Lưu ý: Đổi tên thuộc tính về ReviewerProfile để khớp với Controller của bạn
    public virtual tblReviewerProfiles? ReviewerProfile { get; set; }

    // Lĩnh vực chuyên môn (1-n)
    // Lưu ý: Đổi tên thuộc tính về UserExpertises để khớp với code ở AccountController
    public virtual ICollection<tblUsersExpertise> UserExpertises { get; set; } = new List<tblUsersExpertise>();
}