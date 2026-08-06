
using Microsoft.AspNetCore.Mvc;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

public class VisitorCounterController(VisitorCounterService visitorCounterService) : BaseApiController
{
    [HttpGet]
    public IActionResult GetVisitorCount()
    {
        var count = visitorCounterService.GetCount();
        
        return Ok(new { Count = count });
    }
}

