using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_JMS.Models;
public class tblUsersExpertise
{
    [Key]
    public int ExpertiseId { get; set; }
    [ForeignKey("Users")]
    public int UserId { get; set; }
    public string Keyword { get; set; } = null!;
}