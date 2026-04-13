using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AI_JMS.Models;
public class tblRoles
{
    [Key] // khai báo khoá chính
    public int RoleId { get; set; }
    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = null!;// Author, Reviewer, Editor, EiC, AI
    [StringLength(255)]
    public string? Description { get; set; } = null!;

    // Navigation property (Quan hệ 1-N với UserRoles)
    public virtual ICollection<tblUserRoles> UserRoles { get; set; } = new List<tblUserRoles>();
}