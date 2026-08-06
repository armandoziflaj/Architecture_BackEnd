using Microsoft.AspNetCore.Mvc;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

public class ProjectsController (ProjectService projectService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCatalog([FromHeader(Name = "Accept-Language")] string lang = "en")
    {
        var catalog = await projectService.GetPortfolioCatalogAsync(lang);
        
        return Ok(catalog);
    }
        
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetProjectDetailsAsync(long id,[FromHeader(Name = "Accept-Language")] string lang = "en")
    {
        var catalog = await projectService.GetProjectDetailsAsync(id,lang);
        
        return Ok(catalog);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewProject([FromForm] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var projectId = await projectService.CreateProjectAsync(dto);
        
        return Ok( projectId );
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(long id, [FromForm] UpdateProjectDto dto)
    {
        if (id != dto.Id) return BadRequest("Mismatched Project ID.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        await projectService.UpdateProjectAsync(dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deleteproject(long id)
    {
        if (id == 0) return BadRequest("Missing the project Id");
        
        await projectService.DeleteAsync(id);
        return Ok();
    }
    

}