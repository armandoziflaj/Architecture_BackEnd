using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sulozeqi_BackEnd.Extensions;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Responses;
using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Controller;

public class AuthenticationController(AuthenticationService authService, IConfiguration configuration) : BaseApiController

{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var adminUser = configuration["AdminCredentials:Username"];
        var adminPass = configuration["AdminCredentials:Password"];

        if (request.Username != adminUser || request.Password != adminPass)
        {
            return BadRequest(new { success = false, message = "Invalid credentials." });
        }
        var token = authService.CreateToken(request.Username);
        var duration = int.Parse(configuration["JwtSettings:DurationInMinutes"] ?? "60");
        HttpContext.AppendAuthCookie(token, duration);
        
        return Ok();
    }
    [HttpPost("refresh")]
    [Authorize] 
    public IActionResult Refresh()
    {
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new { success = false, message = "Invalid token claims." });
        }

        var newToken = authService.CreateToken(username);
        
        var duration = int.Parse(configuration["JwtSettings:DurationInMinutes"] ?? "60");
        HttpContext.AppendAuthCookie(newToken, duration);

        return Ok();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.DeleteAuthCookie();
        
        return Ok();
    }
}