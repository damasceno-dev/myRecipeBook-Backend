namespace MyRecipeBook.Domain.Entities;

public class RefreshToken : EntityBase
{
    public string Value { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}