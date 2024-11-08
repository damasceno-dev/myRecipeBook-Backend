namespace MyRecipeBook.Domain.Entities;

public class Instruction : EntityBase
{
    public int Step { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid RecipeId { get; set; }
}