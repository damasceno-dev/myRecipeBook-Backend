using CommonTestUtilities.Entities;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Services;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.Filter;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Exception;
using Xunit;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace UseCases.Test.UseCases.Recipes.Filter;

public class RecipeFilterUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user, imageIdentifierPercentage: 0.8);
        var request = RequestRecipeFilterJsonBuilder.BuildFromList(recipes);
        var useCase = CreateRecipeFilterUseCase(user,recipes,request);
        
        var response =  await useCase.Execute(request);
        
        response.Should().NotBeEmpty();
        response.Should().AllSatisfy(recipe =>
        {
            recipe.Id.Should().NotBeEmpty();
            recipe.Title.Should().Contain(request.TitleIngredient);
            recipe.QuantityIngredients.Should().Be(recipes.First(r => r.Title == request.TitleIngredient).Ingredients.Count);
            var correspondingRecipe = recipes.First(r => r.Id == recipe.Id);
            if (!string.IsNullOrWhiteSpace(correspondingRecipe.ImageIdentifier))
            {
                recipe.ImageUrl.Should().NotBeNullOrWhiteSpace();
                recipe.ImageUrl.Should().MatchRegex(@"\.(jpg|png)$");
            }
            else
            {
                recipe.ImageUrl.Should().BeNullOrWhiteSpace();
            }
        });
    }
    
    [Fact]
    public async void RecipeFilterNotInEnum()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var outOfRangeDishType = (DishType)EnumTestHelper.OutOfRangeEnum<DishType>();
        var outOfRangeDifficulty = (Difficulty)EnumTestHelper.OutOfRangeEnum<Difficulty>();
        var outOfRangeCookingTime = (CookingTime)EnumTestHelper.OutOfRangeEnum<CookingTime>();
        var request = RequestRecipeFilterJsonBuilder.Build();
        request.DishTypes.Add(outOfRangeDishType);
        request.Difficulties.Add(outOfRangeDifficulty);
        request.CookingTimes.Add(outOfRangeCookingTime);
        
        var useCase = CreateRecipeFilterUseCase(user, recipes, request);
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().HaveCount(3);
        exception.And.GetErrors.Should().Contain([
            ResourceErrorMessages.RECIPE_DIFFICULTY_NOT_IN_ENUM,
            ResourceErrorMessages.RECIPE_COOKING_TIME_NOT_IN_ENUM,
            ResourceErrorMessages.RECIPE_DISH_TYPE_NOT_IN_ENUM
        ]);
    }
    private static RecipeFilterUseCase CreateRecipeFilterUseCase(User user, List<Recipe> recipes, RequestRecipeFilterJson request)
    {
        var mapper = MapperBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var recipeRepository = new RecipeRepositoryBuilder().FilterRecipe(recipes,request).Build();
        var storageService = new StorageServiceBuilder().GetFileUrl(recipes).Build();
        return new RecipeFilterUseCase(usersRepository, recipeRepository, mapper,storageService);
    }
}