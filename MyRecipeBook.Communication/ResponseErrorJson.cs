namespace MyRecipeBook.Communication;

public class ResponseErrorJson
{
    public List<string> ErrorMessages { get; set; }
    public ResponseErrorJson(List<string> errors)
    {
        ErrorMessages = errors;
    }

    public ResponseErrorJson(string error)
    {
        ErrorMessages = [error];
    }
}