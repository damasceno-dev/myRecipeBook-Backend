using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Tokens;

public class GetTokenValueFromRequest : ITokenProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public GetTokenValueFromRequest(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
    
    public string Value()
    {
        var authentication = _contextAccessor.HttpContext!.Request.Headers.Authorization.ToString();

        return authentication["Bearer ".Length..].Trim();
    }
}