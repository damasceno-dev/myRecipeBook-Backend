using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Recipes;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Enums;
using Xunit;

namespace Validators.Test.Recipe;

public class RecipeRegisterAndUpdateFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestRecipeJsonBuilder.Build();
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void RecipeTitleEmpty()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_TITLE_EMPTY);
    }
    
    [Fact]
    public void RecipeNotInEnum()
    {
        var outOfRangeDishType = (DishType)EnumTestHelper.OutOfRangeEnum<DishType>();
        var outOfRangeDifficulty = (Difficulty)EnumTestHelper.OutOfRangeEnum<Difficulty>();
        var outOfRangeCookingTime = (CookingTime)EnumTestHelper.OutOfRangeEnum<CookingTime>();
        var request = RequestRecipeJsonBuilder.Build();
        request.DishTypes.Add(outOfRangeDishType);
        request.Difficulty = outOfRangeDifficulty;
        request.CookingTime = outOfRangeCookingTime;
        
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Select(v => v.ErrorMessage)
            .Should().Contain([
                ResourceErrorMessages.RECIPE_DIFFICULTY_NOT_IN_ENUM,
                ResourceErrorMessages.RECIPE_COOKING_TIME_NOT_IN_ENUM,
                ResourceErrorMessages.RECIPE_DISH_TYPE_NOT_IN_ENUM
            ]);
    }

    [Fact]
    public void RecipeIngredientListEmpty()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Ingredients = [];
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_AT_LEAST_ONE_INGREDIENT);
    }
    
    [Fact]
    public void RecipeIngredientItemEmpty()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Ingredients = ["    "];
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_NOT_EMPTY);
    }
    
    [Fact]
    public void RecipeInstructionStepGreaterThanZero()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 0, Text = "Chop onions" }
        };
        
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == ResourceErrorMessages.RECIPE_INSTRUCTION_STEP_GREATER_THAN_0);
    }

    [Fact]
    public void RecipeInstructionTextNotEmpty()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = string.Empty }
        };

        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == ResourceErrorMessages.RECIPE_INSTRUCTION_TEXT_NOT_EMPTY);
    }

    [Fact]
    public void RecipeInstructionTextLessThan2000Characters()
    {
        var overLimitText = new string('a', SharedValidators.MaximumRecipeInstructionTextLength + 1);
        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = overLimitText }
        };

        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == ResourceErrorMessages.RECIPE_INSTRUCTION_TEXT_LESS_THAN_2000);
    }

    [Fact]
    public void RecipeInstructionStepMustBeUnique()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = "Step 1" },
            new() { Step = 1, Text = "Duplicate Step 1" }
        };

        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == ResourceErrorMessages.RECIPE_INSTRUCTION_DUPLICATE_STEP_INSTRUCTION);
    }

}