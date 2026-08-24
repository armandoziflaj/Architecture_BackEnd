using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sulozeqi_BackEnd.Models;

public class ProjectPhoto : CommonData
{
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;

    public bool IsMainCover { get; set; }

    public int DisplayOrder { get; set; }

    public long ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }
}