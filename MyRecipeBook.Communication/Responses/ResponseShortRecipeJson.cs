namespace MyRecipeBook.Communication.Responses;

public class ResponseShortRecipeJson
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuantityIngredients { get; set; }
}