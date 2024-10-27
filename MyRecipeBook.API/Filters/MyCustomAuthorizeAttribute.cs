using Microsoft.AspNetCore.Mvc;

namespace MyRecipeBook.Filters;

public class MyCustomAuthorizeAttribute : TypeFilterAttribute
{
    public MyCustomAuthorizeAttribute() : base(typeof(MyCustomAuthorizeFilter))
    {
    }
}