using Microsoft.AspNetCore.Mvc;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Services;
using System.Text.Json;

namespace Sulozeqi_BackEnd.Controller;

public class ProjectsController (ProjectService projectService) : BaseApiController
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpGet]
    public async Task<IActionResult> GetCatalog(bool onlyFeatured = true,[FromHeader(Name = "Accept-Language")] string lang = "en")
    {
        var catalog = await projectService.GetPortfolioCatalogAsync(onlyFeatured, lang);
        return Ok(catalog);
    }
        
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetProjectDetailsAsync(long id, [FromHeader(Name = "Accept-Language")] string lang = "en")
    {
        var catalog = await projectService.GetProjectDetailsAsync(id, lang);
        return Ok(catalog);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewProject([FromForm] string projectData, [FromForm] List<IFormFile> newPhotos)
    {
        var dto = JsonSerializer.Deserialize<CreateProjectDto>(projectData, _jsonOptions);
        if (dto == null) return BadRequest("Invalid project data.");
        
        dto.NewPhotos = newPhotos;

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var projectId = await projectService.CreateProjectAsync(dto);
        return Ok(new { id = projectId });
    }
    
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateProject(long id, [FromForm] string projectData, [FromForm] List<IFormFile> newPhotos)
    {
        var dto = JsonSerializer.Deserialize<UpdateProjectDto>(projectData, _jsonOptions);
        if (dto == null) return BadRequest("Invalid project data.");
        
        dto.NewPhotos = newPhotos;

        if (id != dto.Id) return BadRequest("Mismatched Project ID.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        await projectService.UpdateProjectAsync(dto);
        return Ok();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteProject(long id)
    {
        if (id == 0) return BadRequest("Missing the project Id");
        
        await projectService.DeleteAsync(id);
        return Ok();
    }
}