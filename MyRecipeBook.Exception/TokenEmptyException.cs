using System.Net;

namespace MyRecipeBook.Exception;

public class TokenEmptyException : MyRecipeBookException
{
    public TokenEmptyException() : base(string.Empty)
    {
    }

    public override int GetStatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors => [Message];
}