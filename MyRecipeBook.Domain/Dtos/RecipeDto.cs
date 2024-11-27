using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Entities.DishType;

namespace MyRecipeBook.Domain.Dtos;

public record RecipeDto(
    string Title,
    CookingTime CookingTime,
    Difficulty Difficulty,
    IList<Ingredient> Ingredients,
    IList<Instruction> Instructions,
    IList<DishType> DishTypes
);