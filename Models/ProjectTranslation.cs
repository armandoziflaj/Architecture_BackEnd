using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Sulozeqi_BackEnd.Models;
public class ProjectTranslation : CommonData
{
    [DataMember]
    [Required]
    public long ProjectId { get; set; }
    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; } = null!;

    [Required]
    [MaxLength(2)]
    public string LanguageCode { get; set; } = "en";

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Summary { get; set; } = string.Empty;
}