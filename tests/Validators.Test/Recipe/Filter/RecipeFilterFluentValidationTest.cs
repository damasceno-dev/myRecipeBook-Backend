using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.Filter;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Enums;
using Xunit;

namespace Validators.Test.Recipe.Filter;

public class RecipeFilterFluentValidationTest
{
    
    [Fact]
    public void Success()
    {
        var request = RequestRecipeFilterJsonBuilder.Build();
        var result = new RecipeFilterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void RecipeFilterNotInEnum()
    {
        var outOfRangeDishType = (DishType)EnumTestHelper.OutOfRangeEnum<DishType>();
        var outOfRangeDifficulty = (Difficulty)EnumTestHelper.OutOfRangeEnum<Difficulty>();
        var outOfRangeCookingTime = (CookingTime)EnumTestHelper.OutOfRangeEnum<CookingTime>();
        var request = RequestRecipeFilterJsonBuilder.Build();
        request.DishTypes.Add(outOfRangeDishType);
        request.Difficulties.Add(outOfRangeDifficulty);
        request.CookingTimes.Add(outOfRangeCookingTime);
        
        var result = new RecipeFilterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Select(v => v.ErrorMessage)
            .Should().Contain([
                ResourceErrorMessages.RECIPE_DIFFICULTY_NOT_IN_ENUM,
                ResourceErrorMessages.RECIPE_COOKING_TIME_NOT_IN_ENUM,
                ResourceErrorMessages.RECIPE_DISH_TYPE_NOT_IN_ENUM
            ]);
    }
}