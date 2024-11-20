using FluentValidation;
using MyRecipeBook.Communication;

namespace MyRecipeBook.Application.UseCases.Recipes.GetRecipes;

public class RecipeGetByUserFluentValidation : AbstractValidator<int>
{
    public RecipeGetByUserFluentValidation()
    {
        RuleFor(x => x).GreaterThan(0).WithMessage(ResourceErrorMessages.RECIPE_NUMBER_GREATER_THAN_0);
    }
}