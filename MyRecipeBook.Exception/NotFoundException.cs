using System.Net;

namespace MyRecipeBook.Exception;

public class NotFoundException(string message) : MyRecipeBookException(message)
{
    public override int GetStatusCode => (int)HttpStatusCode.NotFound;
    public override List<string> GetErrors => [Message];
}