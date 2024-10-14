using System.Net;

namespace MyRecipeBook.Exception;

public class InvalidLoginException : MyRecipeBookException
{
    public InvalidLoginException(string message) : base(message)
    {
    }

    public override int GetStatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors => [Message];
}