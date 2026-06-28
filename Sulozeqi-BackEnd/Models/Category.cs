using System.ComponentModel.DataAnnotations;

namespace Sulozeqi_BackEnd.Models;

public class Category : CommonData
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}