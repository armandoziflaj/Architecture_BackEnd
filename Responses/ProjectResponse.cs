namespace Sulozeqi_BackEnd.Responses;

public class ProjectTranslationResponse
{
    public required string LanguageCode { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
}

public class PhotosResponse
{
    public long Id { get; set; }
    public required string ImageUrl { get; set; }
    public string? AltText { get; set; }
    public bool isCover { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProjectResponse
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public required string Location { get; set; }
    public string? CompletionYear { get; set; }
    public string Size { get; set; } = null!;
    public long? CategoryId { get; set; }
    public List<PhotosResponse> Photos { get; set; } = [];
}

public class ProjectDetailedResponse : ProjectResponse
{
    public string Summary { get; set; } = null!;
    public bool IsFeatured { get; set; }

    public List<ProjectTranslationResponse> Translations { get; set; } = [];
}