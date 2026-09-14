using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aura.Api.Common;

public sealed class ServiceExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ServiceException error) return;
        context.Result = new ObjectResult(new ProblemDetails { Status = error.StatusCode, Detail = error.Message }) { StatusCode = error.StatusCode };
        context.ExceptionHandled = true;
    }
}
