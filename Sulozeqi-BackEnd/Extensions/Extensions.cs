using Microsoft.AspNetCore.Http;

namespace Sulozeqi_BackEnd.Extensions;

public static class HttpContextExtensions
{
    public static void AppendAuthCookie(this HttpContext context, string token, int durationInMinutes)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(durationInMinutes)
        };

        context.Response.Cookies.Append("X-Access-Token", token, cookieOptions);
    }

    public static void DeleteAuthCookie(this HttpContext context)
    {
        context.Response.Cookies.Delete("X-Access-Token");
    }
}