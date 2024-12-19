using CommonTestUtilities.Entities;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Services;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.GetByUser;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.GetByUser;

public class RecipeGetByUserUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = new Random().Next(1, recipes.Count);
        var useCase = CreateRecipeGetByUserUseCase(user,recipes,request);
        
        var response =  await useCase.Execute(request);
        
        response.Should().NotBeNull();
        response.Count.Should().BeLessOrEqualTo(recipes.Count);        
        foreach (var recipe in response)
        {
            var correspondingRecipe = recipes.First(r => r.Id == recipe.Id);
            
            recipe.Id.Should().Be(correspondingRecipe.Id);
            recipe.Title.Should().Be(correspondingRecipe.Title);
            recipe.QuantityIngredients.Should().Be(correspondingRecipe.Ingredients.Count);
        }
        response.All(r => recipes.Any(rc => rc.Id == r.Id && rc.UserId == user.Id)).Should().BeTrue();
    }
    
    [Fact]
    public async Task ErrorQuantityLessThanZero()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = new Random().Next(-100,0);
        var useCase = CreateRecipeGetByUserUseCase(user,recipes,request);
        
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_NUMBER_GREATER_THAN_0);
    }

    private static RecipeGetByUserUseCase CreateRecipeGetByUserUseCase(User user, List<Recipe> recipes, int numberOfRecipes)
    {
        var mapper = MapperBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var recipesRepository = new RecipeRepositoryBuilder().GetByUser(recipes,numberOfRecipes).Build();
        var storageService = new StorageServiceBuilder().GetFileUrl(recipes).Build();

        return new RecipeGetByUserUseCase(recipesRepository, usersRepository, mapper, storageService);
    }
}