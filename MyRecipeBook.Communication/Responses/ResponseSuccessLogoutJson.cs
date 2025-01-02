namespace MyRecipeBook.Communication.Responses;

public class ResponseSuccessLogoutJson(string message)
{
    public string Message { get; set; } = message;
}