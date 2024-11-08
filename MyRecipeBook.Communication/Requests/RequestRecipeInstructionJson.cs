namespace MyRecipeBook.Communication.Requests;

public class RequestRecipeInstructionJson
{
    public int Step { get; set; }
    public string Text { get; set; } = string.Empty;
}