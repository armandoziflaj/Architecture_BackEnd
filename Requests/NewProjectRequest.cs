using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Sulozeqi_BackEnd.Requests;

public class ProjectTranslationDto
{
    [Required]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public class ProjectPhotoSpecDto
{
    public long? Id { get; set; }
    public int DisplayOrder { get; set; }
    public int? NewPhotoIndex { get; set; }
    public string? ImageUrl { get; set; }
}

public class CreateProjectDto
{
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CompletionYear { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Size { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }

    public long? CategoryId { get; set; }

    public List<ProjectTranslationDto> Translations { get; set; } = [];
    
    public List<ProjectPhotoSpecDto> Photos { get; set; } = [];

    [JsonIgnore]
    public List<IFormFile> NewPhotos { get; set; } = [];
}

public class UpdateProjectDto
{
    public long Id { get; set; }

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CompletionYear { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Size { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }

    public long? CategoryId { get; set; }

    public List<ProjectTranslationDto> Translations { get; set; } = [];

    public List<ProjectPhotoSpecDto> Photos { get; set; } = [];

    [JsonIgnore]
    public List<IFormFile> NewPhotos { get; set; } = [];
}