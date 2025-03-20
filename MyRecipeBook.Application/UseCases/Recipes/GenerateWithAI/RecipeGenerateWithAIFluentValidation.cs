using FluentValidation;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;

public class RecipeGenerateWithAIFluentValidation : AbstractValidator<RequestRecipeIngredientsForAIJson>
{
    private const int MaximumRecipeIngredients = SharedValidators.MaximumRecipeIngredients;
    public RecipeGenerateWithAIFluentValidation()
    {
        
        RuleFor(recipe => recipe.Ingredients.Count).GreaterThan(0).WithMessage(ResourceErrorMessages.RECIPE_AT_LEAST_ONE_INGREDIENT);
        RuleFor(r => r.Ingredients.Count).LessThan(MaximumRecipeIngredients)
            .WithMessage(ResourceErrorMessages.RECIPE_INGREDIENT_LIST_MAXIMUM_COUNT);
        RuleForEach(r => r.Ingredients).ValidateIngredient();
    }
}