using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_JMS.Models;
public class tblUsersExpertise
{
    [Key]
    public int ExpertiseId { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual tblUsers? User { get; set; } // Thêm cái này để EF không tự đẻ ra UserId1

    public string Keyword { get; set; } = null!;
}