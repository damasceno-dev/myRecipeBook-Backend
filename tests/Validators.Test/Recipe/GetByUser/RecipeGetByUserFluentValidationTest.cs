using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.GetRecipes;
using MyRecipeBook.Communication;
using Xunit;

namespace Validators.Test.Recipe.GetByUser;

public class RecipeGetByUserFluentValidationTest
{   
    [Fact]
    public void Success()
    {
        var numberOfRecipes = new Random().Next(1, 100);
        var result = new RecipeGetByUserFluentValidation().Validate(numberOfRecipes);
        
        result.IsValid.Should().BeTrue();
    }   
    
    [Fact]
    public void ErrorNumberOfRecipesLessThanZero()
    {
        var numberOfRecipes = new Random().Next(-100, 0);
        var result = new RecipeGetByUserFluentValidation().Validate(numberOfRecipes);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.RECIPE_NUMBER_GREATER_THAN_0);
    }
}