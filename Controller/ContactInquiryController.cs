using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

//[Authorize]

public class ContactInquiriesController(ContactInquiryService inquiryService) : BaseApiController
{ 
    [HttpPost("submit")]
    [AllowAnonymous]
    [EnableRateLimiting("ContactFormPolicy")]
    public async Task<IActionResult> Submit([FromBody] ContactInquiryRequest inquiry)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await inquiryService.SubmitInquiryAsync(inquiry);
        return Ok();
    }

    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, bool onlyUnread = false)
    {
        var inquiries = await inquiryService.GetInquiriesAsync(page, pageSize, onlyUnread);
        return Ok(inquiries);
    }

    [HttpPut("admin/{id:long}/toggle-read")]
    public async Task<IActionResult> ToggleRead(long id)
    {
        var currentStatus = await inquiryService.ToggleReadStatusAsync(id);
        return Ok(new { success = true, isRead = currentStatus });
    }

    [HttpDelete("admin/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await inquiryService.DeleteAsync(id);
        return Ok();
    }
}