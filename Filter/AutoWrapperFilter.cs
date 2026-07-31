using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.Filter;

public class AutoWrapperFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var resultType = objectResult.Value?.GetType();
            if (resultType != null && resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(BaseResponse<>))
            {
                await next();
                return;
            }
            
            var wrappedResponse = new BaseResponse<object>
            {
                Success = true,
                Message = "Success",
                Data = objectResult.Value
            };

            objectResult.Value = wrappedResponse;
        }
        else if (context.Result is StatusCodeResult statusCodeResult)
        {
            var wrappedResponse = new BaseResponse<object>
            {
                Success = statusCodeResult.StatusCode is >= 200 and < 300,
                Message = statusCodeResult.StatusCode is >= 200 and < 300 ? "Success" : "Error",
                Data = null
            };

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = statusCodeResult.StatusCode
            };
        }
        
        await next();
    }
}