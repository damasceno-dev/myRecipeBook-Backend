using System.Net;
using MyRecipeBook.Communication;

namespace MyRecipeBook.Exception;

public class RefreshTokenExpiredException : MyRecipeBookException
{
    public RefreshTokenExpiredException() : base(ResourceErrorMessages.REFRESH_TOKEN_EXPIRED)
    {
    }

    public override int GetStatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors => [Message];
}