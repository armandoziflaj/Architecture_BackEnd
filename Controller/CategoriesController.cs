using Microsoft.AspNetCore.Mvc;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

public class CategoriesController (CategoriesService categoryService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetFilters()
    {
        var categories = await categoryService.GetAllAsync();
        
        return Ok(categories);
    }
}