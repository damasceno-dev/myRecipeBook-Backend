using System.ComponentModel.DataAnnotations;

namespace MyRecipeBook.Domain.Entities;

public class User : EntityBase
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Password { get; set; } = string.Empty;
}