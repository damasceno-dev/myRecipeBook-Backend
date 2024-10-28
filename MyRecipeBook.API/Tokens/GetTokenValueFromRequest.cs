using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

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
        if (_contextAccessor.HttpContext is null)
        {
            throw new ArgumentException("Contexto nulo");
        }
        var authentication = _contextAccessor.HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authentication))
        {
            throw new TokenEmptyException();
        }
        
        return authentication["Bearer ".Length..].Trim();
    }
}