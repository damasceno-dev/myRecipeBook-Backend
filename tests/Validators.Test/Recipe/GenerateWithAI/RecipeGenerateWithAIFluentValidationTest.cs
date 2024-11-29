using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;
using MyRecipeBook.Communication;
using Xunit;

namespace Validators.Test.Recipe.GenerateWithAI;

public class RecipeGenerateWithAIFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);
        
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ErrorIngredientListCountExceeded()
    {
        var numberOfIngredients = SharedValidators.MaximumRecipeIngredients + new Random().Next(1, 20);
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build(numberOfIngredients);        
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_LIST_MAXIMUM_COUNT);
    }
    
    [Fact]
    public void ErrorIngredientEmpty()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = string.Empty;
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_NOT_EMPTY);
    }
    
    [Fact]
    public void ErrorInvalidSeparators()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1-cup-flour";
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_SEPARATORS);
    }

    [Fact]
    public void ErrorInvalidStartCharacter()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "-flour";
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_START_CHARACTER);
    }

    [Fact]
    public void ErrorMaximumWordCountExceeded()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1 cup of very very very fine flour";
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_MAXIMUM_WORD_COUNT);
    }

    [Fact]
    public void ErrorInvalidCharacter()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1 cup @ flour";
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_CHARACTER);
    }
}