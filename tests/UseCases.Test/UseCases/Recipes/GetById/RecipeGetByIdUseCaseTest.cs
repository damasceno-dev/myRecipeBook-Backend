using CommonTestUtilities.Entities;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.GetById;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.GetById;

public class RecipeGetByIdUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = recipes.First().Id;
        var useCase = CreateRecipeGetByIdUseCaseUseCase(user,recipes,request);
        
        var response =  await useCase.Execute(request);
        
        response.Should().NotBeNull();
        response.Id.Should().Be(request);
    }    
    
    [Fact]
    public async Task ErrorRecipeNotFound()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = new Guid();
        var useCase = CreateRecipeGetByIdUseCaseUseCase(user,recipes,request);
        
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_NOT_FOUND);
    }
    
    private static RecipeGetByIdUseCase CreateRecipeGetByIdUseCaseUseCase(User user, List<Recipe> recipes, Guid request)
    {
        var mapper = MapperBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var recipeRepository = new RecipeRepositoryBuilder().GetByIdAsNoTracking(recipes,request).Build();
        return new RecipeGetByIdUseCase(usersRepository, recipeRepository, mapper);
    }
}