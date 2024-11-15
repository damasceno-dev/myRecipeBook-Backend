using FluentValidation;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Recipes.Filter;

public class RecipeFilterFluentValidation : AbstractValidator<RequestRecipeFilterJson>
{
    public RecipeFilterFluentValidation()
    {
        RuleForEach(recipe => recipe.CookingTimes).IsInEnum().WithMessage(ResourceErrorMessages.RECIPE_COOKING_TIME_NOT_IN_ENUM);
        RuleForEach(recipe => recipe.Difficulties).IsInEnum().WithMessage(ResourceErrorMessages.RECIPE_DIFFICULTY_NOT_IN_ENUM);
        RuleForEach(recipe => recipe.DishTypes).IsInEnum().WithMessage(ResourceErrorMessages.RECIPE_DISH_TYPE_NOT_IN_ENUM);
    }
}