using CommonTestUtilities.Entities;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.GetById;
using MyRecipeBook.Application.UseCases.Recipes.Update;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.Update;

public class RecipeUpdateUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var recipeToUpdateId = recipes.First().Id;
        var request = RequestRecipeJsonBuilder.Build();
        var useCase = CreateRecipeUpdateUseCaseUseCase(user,recipes,request, recipeToUpdateId);
        
        var response =  await useCase.Execute(recipeToUpdateId,request);
        var updatedRecipe = recipes.First(r => r.Id == recipeToUpdateId);
        
        response.Should().NotBeNull();
        response.Id.Should().Be(recipeToUpdateId);    
        updatedRecipe.Title.Should().Be(request.Title);
        updatedRecipe.CookingTime.Should().Be(request.CookingTime);
        updatedRecipe.Difficulty.Should().Be(request.Difficulty);
        updatedRecipe.Ingredients.Select(i => i.Item).Should().BeEquivalentTo(request.Ingredients);
        updatedRecipe.Instructions.Select(i => i.Text).Should().BeEquivalentTo(request.Instructions.Select(i => i.Text));
        updatedRecipe.DishTypes.Select(d => d.Type).Should().BeEquivalentTo(request.DishTypes);
    }
    
    [Fact]
    public async Task ErrorRecipeNotFound()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var request = RequestRecipeJsonBuilder.Build();
        var recipeToUpdateId = new Guid();
        var useCase = CreateRecipeUpdateUseCaseUseCase(user,recipes,request, recipeToUpdateId);
        
        var act = async () => await useCase.Execute(recipeToUpdateId,request);
        
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_NOT_FOUND);
    }  
    
    [Fact]
    public async void ErrorRecipeTitleEmpty()
    {
        var (user, _) = UserBuilder.Build();
        var recipes = RecipeBuilder.RecipeCollection(user);
        var recipeToUpdateId = recipes.First().Id;
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;
        var useCase = CreateRecipeUpdateUseCaseUseCase(user,recipes,request, recipeToUpdateId);
        
        var act = async () => await useCase.Execute(recipeToUpdateId,request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_TITLE_EMPTY);
    }
    
    private static RecipeUpdateUseCase CreateRecipeUpdateUseCaseUseCase(User user, List<Recipe> recipes, RequestRecipeJson request, Guid recipeToUpdateId)
    {
        var mapper = MapperBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        
        var recipeToUpdate = recipes.FirstOrDefault(r => r.Id == recipeToUpdateId);
        var recipeRepository = new RecipeRepositoryBuilder()
            .GetById(recipes, recipeToUpdateId)
            .Update(mapper, recipeToUpdate, request)
            .Build();
        
        return new RecipeUpdateUseCase(usersRepository, recipeRepository, mapper, unitOfWork);
    }
}