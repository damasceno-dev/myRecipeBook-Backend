using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.DeleteById;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.DeleteById;

public class RecipeDeleteByIdUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = recipes.First().Id;
        var useCase = CreateRecipeDeleteByIdUseCase(user,recipes,request);
        
        var act = async () => await useCase.Execute(request);
        
        await act.Should().NotThrowAsync();
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_NOT_FOUND);
    }  
    [Fact]
    public async Task ErrorRecipeNotFound()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = new Guid();
        var useCase = CreateRecipeDeleteByIdUseCase(user,recipes,request);
        
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_NOT_FOUND);
    }
    
    private static RecipeDeleteByIdUseCase CreateRecipeDeleteByIdUseCase(User user, List<Recipe> recipes, Guid request)
    {
        var unitOfWorkRepository = UnitOfWorkBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var recipeRepositoryBuilder = new RecipeRepositoryBuilder();
        recipeRepositoryBuilder.GetById(recipes, request);
        recipeRepositoryBuilder.DeleteById(recipes, request);
        var recipeRepository = recipeRepositoryBuilder.Build();
        return new RecipeDeleteByIdUseCase(usersRepository, recipeRepository, unitOfWorkRepository);
    }
}