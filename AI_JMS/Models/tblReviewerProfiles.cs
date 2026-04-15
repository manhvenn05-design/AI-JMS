using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AI_JMS.Models;

public class tblReviewerProfiles
{
    [Key]
    public int ProfileId { get; set; }

    public int UserId { get; set; } // Khóa ngoại

    // Phải có thuộc tính điều hướng ngược lại và chỉ rõ ForeignKey
    [ForeignKey("UserId")]
    public virtual tblUsers? User { get; set; } 

    public string? AcademicTitle { get; set; }
    public string? Affiliation { get; set; }
    public int ReviewCount { get; set; }
}