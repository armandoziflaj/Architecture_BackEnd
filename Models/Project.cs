using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sulozeqi_BackEnd.Models;

public class Project : CommonData
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Summary { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CompletionYear { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Size { get; set; } = string.Empty;

    public bool IsFeatured { get; set; } = false;

    public long? CategoryId { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public ICollection<ProjectPhoto> Photos { get; set; } = new List<ProjectPhoto>();
}