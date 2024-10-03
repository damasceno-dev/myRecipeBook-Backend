using System.Net;

namespace MyRecipeBook.Exception;

public class OnValidationException : MyRecipeBookException
{
    private readonly List<string> _errorsMessages;

    public OnValidationException(List<string> errorsMessage) : base(string.Empty)
    {
        _errorsMessages = errorsMessage;
    }
    public override int GetStatusCode => (int)HttpStatusCode.BadRequest;

    public override List<string> GetErrors => _errorsMessages;
}