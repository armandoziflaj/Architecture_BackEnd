using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

public class ExistingPhotoDto
{
    public long Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
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

    public List<IFormFile> Photos { get; set; } = [];

    public List<int> DisplayOrders { get; set; } = [];
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

    public List<ExistingPhotoDto> RetainedPhotos { get; set; } = [];

    public List<IFormFile> NewPhotos { get; set; } = [];

    public List<int> NewPhotoDisplayOrders { get; set; } = [];
}