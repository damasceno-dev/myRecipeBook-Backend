using System.ComponentModel.DataAnnotations;

namespace MyRecipeBook.Domain.Entities;

public class Instruction : EntityBase
{
    public int Step { get; set; }
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;
    public Guid RecipeId { get; set; }
}