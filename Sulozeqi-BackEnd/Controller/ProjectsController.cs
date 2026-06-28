using Microsoft.AspNetCore.Mvc;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

public class ProjectsController (ProjectService projectService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCatalog()
    {
        var catalog = await projectService.GetPortfolioCatalogAsync();
        
        return Ok(catalog);
    }
        
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetProjectDetailsAsync(long id)
    {
        var catalog = await projectService.GetProjectDetailsAsync(id);
        
        return Ok(catalog);
    }
}