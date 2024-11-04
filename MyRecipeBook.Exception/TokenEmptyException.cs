using System.Net;
using MyRecipeBook.Communication;

namespace MyRecipeBook.Exception;

public class TokenEmptyException : MyRecipeBookException
{
    public TokenEmptyException() : base(ResourceErrorMessages.TOKEN_EMPTY)
    {
    }

    public override int GetStatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors => [Message];
}