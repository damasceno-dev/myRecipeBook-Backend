using FluentValidation;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;

public class RecipeGenerateWithAIFluentValidation : AbstractValidator<RequestRecipeIngredientsForAIJson>
{
    public RecipeGenerateWithAIFluentValidation()
    {
        RuleFor(r => r.Ingredients.Count).InclusiveBetween(1, SharedValidators.MaximumRecipeIngredients);
        RuleForEach(r => r.Ingredients).ValidateIngredient();
    }
}