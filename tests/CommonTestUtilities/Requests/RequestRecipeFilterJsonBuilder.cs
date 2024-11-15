using Bogus;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace CommonTestUtilities.Requests;

public class RequestRecipeFilterJsonBuilder
{
    public static RequestRecipeFilterJson Build()
    {
        var cookingTimeListSize = new Random().Next(0, EnumTestHelper.EnumRange<CookingTime>());
        var difficultyListSize = new Random().Next(0, EnumTestHelper.EnumRange<Difficulty>());
        var dishTypeListSize = new Random().Next(0, EnumTestHelper.EnumRange<DishType>());
        return new Faker<RequestRecipeFilterJson>()
            .RuleFor(filter => filter.TitleIngredient, f => f.Commerce.ProductName())
            .RuleFor(filter => filter.CookingTimes, f => f.Make(cookingTimeListSize, f.PickRandom<CookingTime>))
            .RuleFor(filter => filter.Difficulties, f => f.Make(difficultyListSize, f.PickRandom<Difficulty>))
            .RuleFor(filter => filter.DishTypes, f => f.Make(dishTypeListSize, f.PickRandom<DishType>));
    }

    /// <summary>
    /// Picks a random item from a recipe list and return it as a filter request
    /// </summary>
    /// <param name="recipes">Recipe list</param>
    /// <returns>Request from recipe list</returns>
    public static RequestRecipeFilterJson BuildFromList(List<Recipe> recipes)
    {
        var randomRecipe = recipes[new Random().Next(recipes.Count)];

        return new RequestRecipeFilterJson
        {
            TitleIngredient = randomRecipe.Title,
            CookingTimes = randomRecipe.CookingTime.HasValue 
                ? new List<CookingTime> { randomRecipe.CookingTime.Value } 
                : [],
            Difficulties = randomRecipe.Difficulty.HasValue 
                ? new List<Difficulty> { randomRecipe.Difficulty.Value } 
                : [],
            DishTypes = randomRecipe.DishTypes?.Select(d => d.Type).ToList() ?? []
        };
    }

}