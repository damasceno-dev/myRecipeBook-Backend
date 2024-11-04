using Microsoft.AspNetCore.Mvc;

namespace MyRecipeBook.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MyCustomAuthorizeAttribute : TypeFilterAttribute
{
    public MyCustomAuthorizeAttribute() : base(typeof(MyCustomAuthorizeFilter))
    {
    }
}