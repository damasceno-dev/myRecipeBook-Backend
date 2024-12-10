namespace MyRecipeBook.Communication.Responses;

public class ResponseShortRecipeJson
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int QuantityIngredients { get; init; }
    public string? ImageUrl { get; set; }
}