using Microsoft.AspNetCore.Http;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestRecipeFormBuilder
{
    public static RequestRecipeForm Build(IFormFile? file = null)
    {
        var recipeJson = RequestRecipeJsonBuilder.Build();
        return new RequestRecipeForm
        {
            Title = recipeJson.Title,
            CookingTime = recipeJson.CookingTime,
            Difficulty = recipeJson.Difficulty,
            Ingredients = recipeJson.Ingredients,
            Instructions = recipeJson.Instructions,
            DishTypes = recipeJson.DishTypes,
            ImageFile = file
        };
    }
}