namespace MyRecipeBook.Communication.Responses;

public class ResponseRegisteredRecipeJson
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}