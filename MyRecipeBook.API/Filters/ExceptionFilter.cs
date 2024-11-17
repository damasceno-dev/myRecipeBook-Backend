using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var environment = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (context.Exception is MyRecipeBookException exception)
        {
            context.HttpContext.Response.StatusCode = exception.GetStatusCode;
            context.Result = new ObjectResult(new ResponseErrorJson(exception.GetErrors)
            {
                Method = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            });
        }
        else
        {;
            var errorMessage = environment?.EnvironmentName == "Development" ? context.Exception.Message : ResourceErrorMessages.UNKOWN_ERROR;
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new ObjectResult(new ResponseErrorJson(errorMessage)
            {
                Method = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            });
        }
    }
}