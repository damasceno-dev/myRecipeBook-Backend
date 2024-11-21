namespace MyRecipeBook.Communication.Responses;

public class ResponseErrorJson
{
    public List<string> ErrorMessages { get; set; }
    public string Method { get; set; } = string.Empty;
    public ResponseErrorJson(List<string> errors)
    {
        ErrorMessages = errors;
    }

    public ResponseErrorJson(string error)
    {
        ErrorMessages = [error];
    }
}