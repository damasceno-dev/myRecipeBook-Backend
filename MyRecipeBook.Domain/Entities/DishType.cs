namespace MyRecipeBook.Domain.Entities;

public class DishType : EntityBase
{
    public Enums.DishType Type { get; set; }
    public Guid RecipeId { get; set; }
}