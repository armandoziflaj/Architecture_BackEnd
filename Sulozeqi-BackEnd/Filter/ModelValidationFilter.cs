using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.Filter;

public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument != null) continue;
            context.Result = new BadRequestObjectResult(new BaseResponse<object>
            {
                Success = false,
                Message = "The request payload cannot be completely null or empty.",
                Data = null
            });
            return;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) {}
}