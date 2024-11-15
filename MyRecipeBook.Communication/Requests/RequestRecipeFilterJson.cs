using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Communication.Requests;

public class RequestRecipeFilterJson
{
    public string TitleIngredient { get; set; } = string.Empty;
    public IList<CookingTime> CookingTimes { get; set; } = [];
    public IList<Difficulty> Difficulties { get; set; } = [];
    public IList<DishType> DishTypes { get; set; } = [];
}