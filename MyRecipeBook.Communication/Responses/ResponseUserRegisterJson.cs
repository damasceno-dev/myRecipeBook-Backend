namespace MyRecipeBook.Communication.Responses;

public class ResponseUserRegisterJson
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}