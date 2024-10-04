namespace MyRecipeBook.Domain.Entities;

public class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}