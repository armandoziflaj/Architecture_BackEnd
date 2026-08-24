using Microsoft.EntityFrameworkCore;
using Sulozeqi_BackEnd.ExceptionMiddleware;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.Services;

public class ProjectService(AppDbContext context, IWebHostEnvironment environment) : BaseService<Project>(context)
{
    public async Task<IEnumerable<ProjectResponse>> GetPortfolioCatalogAsync(bool onlyFeatured, string lang = "en")
    {
        var query = Context.Projects.AsNoTracking();

        if (onlyFeatured)
        {
            query = query.Where(x => x.IsFeatured);
        }
            
    
        var projects = await query
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

    public async Task<long> CreateProjectAsync(CreateProjectDto dto)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var project = new Project
            {
                Location = dto.Location,
                CompletionYear = dto.CompletionYear,
                Size = dto.Size,
                IsFeatured = dto.IsFeatured,
                CategoryId = dto.CategoryId,
                Translations = dto.Translations.Select(t => new ProjectTranslation
                {
                    LanguageCode = t.LanguageCode,
                    Title = t.Title,
                    Summary = string.IsNullOrWhiteSpace(t.Description) ? t.Title : t.Description
                }).ToList()
            };

            Context.Projects.Add(project);
            await Context.SaveChangesAsync();

            for (var i = 0; i < dto.Photos.Count; i++)
            {
                var photoSpec = dto.Photos[i];
                var file = dto.NewPhotos[i];
                var imageUrl = await SaveImageFileAsync(file);

                project.Photos.Add(new ProjectPhoto
                {
                    ImageUrl = imageUrl,
                    AltText = $"{dto.Location} Project Photo {photoSpec.DisplayOrder}",
                    DisplayOrder = photoSpec.DisplayOrder,
                    IsMainCover = (photoSpec.DisplayOrder == 1)
                });
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            return project.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateProjectAsync(UpdateProjectDto dto)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var project = await Context.Projects
                .Include(p => p.Translations)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new NotFoundException($"Project with ID {dto.Id} not found.");

            ApplyProjectFields(project, dto);
            ReplaceTranslations(project, dto);
            await SyncPhotosAsync(project, dto);

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void ApplyProjectFields(Project project, UpdateProjectDto dto)
    {
        project.Location = dto.Location;
        project.CompletionYear = dto.CompletionYear;
        project.Size = dto.Size;
        project.IsFeatured = dto.IsFeatured;
        project.CategoryId = dto.CategoryId;
    }

    private static void ReplaceTranslations(Project project, UpdateProjectDto dto)
    {
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
    }

    private async Task SyncPhotosAsync(Project project, UpdateProjectDto dto)
    {
        var retainedPhotoIds = dto.Photos.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).ToList();
        var photosToRemove = project.Photos.Where(p => !retainedPhotoIds.Contains(p.Id)).ToList();

        foreach (var photo in photosToRemove)
        {
            DeleteImageFile(photo.ImageUrl);
            Context.ProjectPhotos.Remove(photo);
        }

        foreach (var photoSpec in dto.Photos)
        {
            if (photoSpec.Id.HasValue)
            {
                var existingPhoto = project.Photos.FirstOrDefault(p => p.Id == photoSpec.Id.Value);
                if (existingPhoto != null)
                {
                    existingPhoto.DisplayOrder = photoSpec.DisplayOrder;
                    existingPhoto.IsMainCover = (photoSpec.DisplayOrder == 1);
                }
            }
            else if (photoSpec.NewPhotoIndex.HasValue)
            {
                var file = dto.NewPhotos[photoSpec.NewPhotoIndex.Value];
                var imageUrl = await SaveImageFileAsync(file);
                project.Photos.Add(new ProjectPhoto
                {
                    ImageUrl = imageUrl,
                    AltText = $"{dto.Location} Project Photo {photoSpec.DisplayOrder}",
                    DisplayOrder = photoSpec.DisplayOrder,
                    IsMainCover = (photoSpec.DisplayOrder == 1)
                });
            }
        }
    }

    public override async Task<bool> DeleteAsync(long id)
    {
        var project = await Context.Projects
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} does not exist.");
        }

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
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var relativePath = imageUrl.TrimStart('/');
        var fullPath = Path.Combine(environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    #endregion
}