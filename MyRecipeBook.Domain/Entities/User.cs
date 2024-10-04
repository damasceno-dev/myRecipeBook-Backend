using System.ComponentModel.DataAnnotations;

namespace MyRecipeBook.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Password { get; set; } = string.Empty;
}