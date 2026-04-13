using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AI_JMS.Models;

public class tblReviewerProfiles
{
    [Key] // khai báo khoá chính
    public int ProfileId { get; set; }
    [ForeignKey("Users")]
    public int UserId { get; set; }

    public string AcademicTitle { get; set; } = null!;

    public string Affiliation { get; set; } = null!;

    public int ReviewCount { get; set; } 
}
