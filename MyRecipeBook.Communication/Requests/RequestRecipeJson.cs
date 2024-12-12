using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;
using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace MyRecipeBook.Communication.Requests;

public class RequestRecipeJson
{
    public string Title { get; set; } = string.Empty;
    public CookingTime? CookingTime { get; set; }
    public Difficulty? Difficulty { get; set; }
    public IList<string> Ingredients { get; set; } = [];
    [ModelBinder(typeof(JsonModelBinder))]
    public IList<RequestRecipeInstructionJson> Instructions { get; set; } = [];
    public IList<DishType> DishTypes { get; set; } = [];
}