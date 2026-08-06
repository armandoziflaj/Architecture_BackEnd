using Sulozeqi_BackEnd.Services;

namespace Sulozeqi_BackEnd.Middleware;

public class VisitorCounterMiddleware(RequestDelegate next)
{
    private const string VisitorCookieName = "HasVisited";

    public async Task InvokeAsync(HttpContext context, VisitorCounterService counterService)
    {
        if (!context.Request.Cookies.ContainsKey(VisitorCookieName))
        {
            context.Response.Cookies.Append(VisitorCookieName, "true", new CookieOptions
            {
                HttpOnly = true,
                Expires = null,
                SameSite = SameSiteMode.Lax
            });

            counterService.Increment();
        }

        await next(context);
    }
}


