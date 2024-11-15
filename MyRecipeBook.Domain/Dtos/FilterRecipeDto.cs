using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Dtos;

public record FilterRecipeDto (
    string TitleIngredient, 
    IList<CookingTime> CookingTimes, 
    IList<Difficulty> Difficulties, 
    IList<DishType> DishTypes
    );