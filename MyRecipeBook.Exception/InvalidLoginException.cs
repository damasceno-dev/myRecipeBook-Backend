using System.Net;

namespace MyRecipeBook.Exception;

public class InvalidLoginException : MyRecipeBookException
{
    public string? ErrorCode { get; }

    public InvalidLoginException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public override int GetStatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors => [Message];
}