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
        if (context.Exception is MyRecipeBookException exception)
        {
            context.HttpContext.Response.StatusCode = exception.GetStatusCode;
            context.Result = new ObjectResult(new ResponseErrorJson(exception.GetErrors)
            {
                Method = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            });
        }
        else
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new ObjectResult(new ResponseErrorJson(ResourceErrorMessages.UNKOWN_ERROR)
            {
                Method = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            });
        }
    }
}