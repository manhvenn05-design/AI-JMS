using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AI_JMS.Models;

public class tblMenus
{
    [Key] 
    public int MenuId { get; set; }
    [Required]
    [StringLength(100)]
    public string MenuName { get; set; } = null!;

    public string? ActionName { get; set; } = null!;

    public string? ControllerName { get; set; } = null!;
    public string? AreaName { get; set; } = null!;
    [StringLength(50)]
    public string? Icon { get; set; } = null!;

    public int? ParentId { get; set; } 

    public int DisplayOrder { get; set; }  
    [ForeignKey("Roles")]
    // Ràng buộc menu này chỉ hiện cho một Role nhất định
    public int? RequiredRoleId { get; set; }
    [ForeignKey("RequiredRoleId")]
    public virtual tblRoles? Role { get; set; }
}