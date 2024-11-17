using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace MyRecipeBook.Communication.Responses;

public class ResponseRecipeJson
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public CookingTime? CookingTime { get; set; }
    public Difficulty? Difficulty { get; set; }
    public IList<string> Ingredients { get; set; } = [];
    public IList<ResponseInstructionJson> Instructions { get; set; } = [];
    public IList<DishType> DishTypes { get; set; } = [];
}