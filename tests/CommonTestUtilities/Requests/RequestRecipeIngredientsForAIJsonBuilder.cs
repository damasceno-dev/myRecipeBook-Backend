using Bogus;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestRecipeIngredientsForAIJsonBuilder
{
    public static RequestRecipeIngredientsForAIJson Build(int numberOfIngredients = 0)
    {
        var ingredientNumber = numberOfIngredients > 0 ? 
            numberOfIngredients : 
            new Random().Next(1, SharedValidators.MaximumRecipeIngredients);
        
        return new Faker<RequestRecipeIngredientsForAIJson>()
            .RuleFor(recipe => recipe.Ingredients, f => f.Make(ingredientNumber, () => f.Commerce.Product()));
    }
}