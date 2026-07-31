using Microsoft.EntityFrameworkCore;
using Sulozeqi_BackEnd.ExceptionMiddleware;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.Services;

public class ProjectService(AppDbContext context, IWebHostEnvironment environment) : BaseService<Project>(context)
{
    public async Task<IEnumerable<ProjectResponse>> GetPortfolioCatalogAsync(string lang = "en")
    {
        var projects = await Context.Projects
            .AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Photos.Where(photo => photo.IsMainCover))
            .ToListAsync();

        return projects.Select(p => new ProjectResponse
        {
            Id = p.Id,
            Title = ResolveTitle(p.Translations, lang),
            Location = p.Location,
            CompletionYear = p.CompletionYear,
            Size = p.Size,
            CategoryId = p.CategoryId,
            Photos = p.Photos
                .Select(photo => new PhotosResponse
                {
                    Id = photo.Id,
                    ImageUrl = photo.ImageUrl,
                    AltText = photo.AltText,
                    isCover = photo.IsMainCover
                })
                .ToList()
        });
    }

    public async Task<ProjectDetailedResponse> GetProjectDetailsAsync(long id, string lang = "en")
    {
        var project = await Context.Projects
                          .AsNoTracking()
                          .Include(p => p.Translations)
                          .Include(p => p.Photos)
                          .FirstOrDefaultAsync(p => p.Id == id) 
                      ?? throw new NotFoundException($"Project with ID {id} does not exist.");

        return new ProjectDetailedResponse
        {
            Id = project.Id,
            Title = ResolveTitle(project.Translations, lang),
            Summary = ResolveSummary(project.Translations, lang),
            Location = project.Location,
            CompletionYear = project.CompletionYear,
            Size = project.Size,
            CategoryId = project.CategoryId,
            IsFeatured = project.IsFeatured,

            Translations = project.Translations.Select(t => new ProjectTranslationResponse
            {
                LanguageCode = t.LanguageCode,
                Title = t.Title,
                Summary = t.Summary
            }).ToList(),

            Photos = project.Photos
                .OrderBy(photo => photo.DisplayOrder)
                .Select(photo => new PhotosResponse
                {
                    Id = photo.Id,
                    ImageUrl = photo.ImageUrl,
                    AltText = photo.AltText,
                    isCover = photo.IsMainCover,
                    DisplayOrder = photo.DisplayOrder
                })
                .ToList()
        };
    }

    public async Task<long> CreateProjectAsync(CreateProjectDto request)
    {
        var project = new Project
        {
            Location = request.Location,
            CompletionYear = request.CompletionYear,
            Size = request.Size,
            IsFeatured = request.IsFeatured,
            CategoryId = request.CategoryId
        };

        foreach (var translationDto in request.Translations)
        {
            project.Translations.Add(new ProjectTranslation
            {
                LanguageCode = translationDto.LanguageCode,
                Title = translationDto.Title,
                Summary = string.IsNullOrWhiteSpace(translationDto.Description)
                    ? translationDto.Title
                    : translationDto.Description
            });
        }

        for (var i = 0; i < request.Photos.Count; i++)
        {
            var file = request.Photos[i];
            if (file.Length == 0) continue;

            var imageUrl = await SaveImageFileAsync(file);
            var displayOrder = (request.DisplayOrders != null && i < request.DisplayOrders.Count)
                ? request.DisplayOrders[i]
                : i + 1;

            project.Photos.Add(new ProjectPhoto
            {
                ImageUrl = imageUrl,
                AltText = $"{request.Location} Project Photo {displayOrder}",
                DisplayOrder = displayOrder,
                IsMainCover = (displayOrder == 1)
            });
        }

        Context.Projects.Add(project);
        await Context.SaveChangesAsync();

        return project.Id;
    }

    public async Task UpdateProjectAsync(UpdateProjectDto dto)
    {
        var project = await Context.Projects
            .Include(p => p.Translations)
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == dto.Id);

        if (project == null)
            throw new NotFoundException($"Project with ID {dto.Id} not found.");

        project.Location = dto.Location;
        project.CompletionYear = dto.CompletionYear;
        project.Size = dto.Size;
        project.IsFeatured = dto.IsFeatured;
        project.CategoryId = dto.CategoryId;

        project.Translations.Clear();
        foreach (var transDto in dto.Translations)
        {
            project.Translations.Add(new ProjectTranslation
            {
                LanguageCode = transDto.LanguageCode,
                Title = transDto.Title,
                Summary = transDto.Description
            });
        }

        var retainedIds = dto.RetainedPhotos.Select(rp => rp.Id).ToList();
        var photosToRemove = project.Photos.Where(p => !retainedIds.Contains(p.Id)).ToList();

        foreach (var photo in photosToRemove)
        {
            DeleteImageFile(photo.ImageUrl);
            Context.ProjectPhotos.Remove(photo);
        }

        foreach (var photo in project.Photos)
        {
            var match = dto.RetainedPhotos.FirstOrDefault(rp => rp.Id == photo.Id);
            if (match != null)
            {
                photo.DisplayOrder = match.DisplayOrder;
            }
        }

        for (var i = 0; i < dto.NewPhotos.Count; i++)
        {
            var file = dto.NewPhotos[i];
            if (file.Length == 0) continue;

            var imageUrl = await SaveImageFileAsync(file);
            var displayOrder = (dto.NewPhotoDisplayOrders != null && i < dto.NewPhotoDisplayOrders.Count)
                ? dto.NewPhotoDisplayOrders[i]
                : i + 100;

            project.Photos.Add(new ProjectPhoto
            {
                ImageUrl = imageUrl,
                AltText = $"{dto.Location} Project Photo {displayOrder}",
                DisplayOrder = displayOrder,
                IsMainCover = false
            });
        }

        foreach (var photo in project.Photos)
        {
            photo.IsMainCover = (photo.DisplayOrder == 1);
        }

        await Context.SaveChangesAsync();
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        var project = await Context.Projects
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            throw new NotFoundException($"Project with ID {id} does not exist.");

        foreach (var photo in project.Photos)
        {
            DeleteImageFile(photo.ImageUrl);
        }

        Context.Projects.Remove(project);
        await Context.SaveChangesAsync();

        return true;
    }

    #region Private Helpers

    private static string ResolveTitle(IEnumerable<ProjectTranslation> translations, string lang)
    {
        var list = translations.ToList();
        return list.FirstOrDefault(t => t.LanguageCode == lang)?.Title
            ?? list.FirstOrDefault(t => t.LanguageCode == "en")?.Title
            ?? string.Empty;
    }

    private static string ResolveSummary(IEnumerable<ProjectTranslation> translations, string lang)
    {
        var list = translations.ToList();
        return list.FirstOrDefault(t => t.LanguageCode == lang)?.Summary
            ?? list.FirstOrDefault(t => t.LanguageCode == "en")?.Summary
            ?? string.Empty;
    }

    private async Task<string> SaveImageFileAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "projects");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/projects/{uniqueFileName}";
    }

    private void DeleteImageFile(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;

        var relativePath = imageUrl.TrimStart('/');
        var fullPath = Path.Combine(environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    #endregion
}