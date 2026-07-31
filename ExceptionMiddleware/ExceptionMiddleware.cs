using System.Net;
using System.Text.Json;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.ExceptionMiddleware;


public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception was intercepted by the global handler.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected server error occurred.";

        switch (exception)
        {
            case BadRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest; 
                message = badRequestEx.Message;
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;
                
            default:
                #if DEBUG
                message = exception.Message;
                #endif
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new BaseResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}