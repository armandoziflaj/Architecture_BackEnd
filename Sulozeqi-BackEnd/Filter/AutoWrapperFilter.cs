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
            if (resultType != null && resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(BaseApiResponse<>))
            {
                await next();
                return;
            }
            
            var wrappedResponse = new BaseApiResponse<object>
            {
                Success = true,
                Message = "Success",
                Data = objectResult.Value
            };

            objectResult.Value = wrappedResponse;
        }

        await next();
    }
}