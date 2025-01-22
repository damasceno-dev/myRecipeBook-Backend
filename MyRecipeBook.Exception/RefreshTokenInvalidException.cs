using System.Net;
using MyRecipeBook.Communication;

namespace MyRecipeBook.Exception;

public class RefreshTokenInvalidException : MyRecipeBookException
{
    public RefreshTokenInvalidException() : base(ResourceErrorMessages.REFRESH_TOKEN_INVALID)
    {
    }

    public override int GetStatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors => [Message];
}