using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

[Authorize]
[EnableRateLimiting("ContactFormPolicy")]
public class ContactInquiriesController(ContactInquiryService inquiryService) : BaseApiController
{ 
    [HttpPost("submit")]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] ContactInquiry inquiry)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await inquiryService.SubmitInquiryAsync(inquiry);
        return Ok();
    }

    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAll()
    {
        var inquiries = await inquiryService.GetAllInquiriesAsync();
        return Ok(inquiries);
    }

    [HttpPut("admin/{id:long}/toggle-read")]
    public async Task<IActionResult> ToggleRead(long id)
    {
        var currentStatus = await inquiryService.ToggleReadStatusAsync(id);
        return Ok(new { success = true, isRead = currentStatus });
    }
}