using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AI_JMS.Models;

public class tblSystemconfigs
{
    [Key]
    public int ConfigKey { get; set; }
    public string ConfigValue { get; set; } = null!;
    public string Description { get; set; } = null!;

}