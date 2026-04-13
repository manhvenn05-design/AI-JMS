using System.ComponentModel.DataAnnotations;
namespace AI_JMS.Models;
public class tblAnnouncements
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime PublishedDate { get; set; }

    public Boolean IsActive { get; set; } 

}