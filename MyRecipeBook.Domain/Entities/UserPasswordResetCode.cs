namespace MyRecipeBook.Domain.Entities;

public class UserPasswordResetCode : EntityBase
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}