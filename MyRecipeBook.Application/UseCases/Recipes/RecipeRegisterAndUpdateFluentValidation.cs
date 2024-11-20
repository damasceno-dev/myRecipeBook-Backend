using FluentValidation;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Recipes;

public class RecipeRegisterAndUpdateFluentValidation : AbstractValidator<RequestRecipeJson>
{
    private const int MaximumInstructionTextLength = SharedValidators.MaximumRecipeInstructionTextLength;
    public RecipeRegisterAndUpdateFluentValidation()
    {
        RuleFor(recipe => recipe.Title).NotEmpty().WithMessage(ResourceErrorMessages.RECIPE_TITLE_EMPTY);
        RuleFor(recipe => recipe.Difficulty).IsInEnum().WithMessage(ResourceErrorMessages.RECIPE_DIFFICULTY_NOT_IN_ENUM);
        RuleFor(recipe => recipe.CookingTime).IsInEnum().WithMessage(ResourceErrorMessages.RECIPE_COOKING_TIME_NOT_IN_ENUM);
        RuleFor(recipe => recipe.Ingredients.Count).GreaterThan(0).WithMessage(ResourceErrorMessages.RECIPE_AT_LEAST_ONE_INGREDIENT);
        RuleForEach(recipe => recipe.Ingredients).NotEmpty().WithMessage(ResourceErrorMessages.RECIPE_INGREDIENT_NOT_EMPTY);
        RuleForEach(recipe => recipe.Instructions).ChildRules(instructionRule =>
        {
            instructionRule.RuleFor(instruction => instruction.Step).GreaterThan(0).WithMessage(ResourceErrorMessages.RECIPE_INSTRUCTION_STEP_GREATER_THAN_0);
            instructionRule.RuleFor(instruction => instruction.Text).NotEmpty().WithMessage(ResourceErrorMessages.RECIPE_INSTRUCTION_TEXT_NOT_EMPTY);
            instructionRule.RuleFor(instruction => instruction.Text).MaximumLength(MaximumInstructionTextLength).WithMessage(ResourceErrorMessages.RECIPE_INSTRUCTION_TEXT_LESS_THAN_2000);
        });
        RuleFor(recipe => recipe.Instructions).Must(instructions => 
            instructions.Select(i => i.Step).Distinct().Count() == instructions.Count)
            .WithMessage(ResourceErrorMessages.RECIPE_INSTRUCTION_DUPLICATE_STEP_INSTRUCTION);
        RuleForEach(recipe => recipe.DishTypes).IsInEnum().WithMessage(ResourceErrorMessages.RECIPE_DISH_TYPE_NOT_IN_ENUM);
        
        
    }
}