using System.Net;

namespace MyRecipeBook.Exception;

public class ConflictException : MyRecipeBookException
{
    public ConflictException(string message) : base(message)
    {
    }

    public override int GetStatusCode => (int)HttpStatusCode.Conflict;

    public override List<string> GetErrors => [Message];
}