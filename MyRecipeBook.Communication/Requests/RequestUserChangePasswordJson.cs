namespace MyRecipeBook.Communication.Requests;

public class RequestUserChangePasswordJson
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}