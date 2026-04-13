using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AI_JMS.Models;

public class tblUserRoles
{

   public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual tblUsers User { get; set; } = null!;

    public int RoleId { get; set; }
    [ForeignKey("RoleId")]
    public virtual tblRoles Role { get; set; } = null!;
}