using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AI_JMS.Models;

public class tblUsers
{
    [Key] // khai báo khoá chính
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;
    [Required]
    public string PasswordHash { get; set; } = null!;
    public string? Avatar { get; set; } = null!;

    [StringLength(10)]
    public string PreferredLanguage { get; set; } = "vi";
    public int? LastSelectedRoleId { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<tblUserRoles> UserRoles { get; set; } = new List<tblUserRoles>();
}