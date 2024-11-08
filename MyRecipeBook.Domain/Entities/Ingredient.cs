namespace MyRecipeBook.Domain.Entities;

public class Ingredient : EntityBase
{
    public string Item { get; set; } = string.Empty;
    public Guid RecipeId { get; set; }
}