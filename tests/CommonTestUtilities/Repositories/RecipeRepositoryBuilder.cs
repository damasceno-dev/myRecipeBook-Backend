using AutoMapper;
using Moq;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Infrastructure.Repositories;

namespace CommonTestUtilities.Repositories;

public class RecipeRepositoryBuilder
{
    private readonly Mock<IRecipesRepository> _repository;

    public RecipeRepositoryBuilder()
    {
        _repository = new Mock<IRecipesRepository>();
    }

    public RecipeRepositoryBuilder FilterRecipe(List<Recipe> recipes, RequestRecipeFilterJson request)
    {
        var filterDto = new FilterRecipeDto
        (
            TitleIngredient:request.TitleIngredient,
            CookingTimes:request.CookingTimes,
            Difficulties:request.Difficulties,
            DishTypes:request.DishTypes
        );

        _repository
            .Setup(repo => repo.FilterRecipe(It.IsAny<User>(), It.IsAny<FilterRecipeDto>()))
            .ReturnsAsync(() => RecipesRepository.RecipeFilterLogic(recipes.AsQueryable(), filterDto).ToList());

        return this;
    }

    public RecipeRepositoryBuilder GetByIdAsNoTracking(List<Recipe> recipes, Guid recipeId)
    {
        _repository.Setup(repo => repo.GetByIdAsNoTracking(It.IsAny<User>(), It.IsAny<Guid>()))
            .ReturnsAsync((User user,Guid id) => recipes.AsQueryable().FirstOrDefault(r => r.Id == id));
        
        return this;
    }
    public RecipeRepositoryBuilder GetById(List<Recipe> recipes, Guid recipeId)
    {
        _repository.Setup(repo => repo.GetById(It.IsAny<User>(), It.IsAny<Guid>()))
            .ReturnsAsync((User user,Guid id) => recipes.AsQueryable().FirstOrDefault(r => r.Id == id));
        
        return this;
    }
    public RecipeRepositoryBuilder GetById(Recipe recipe)
    {
        _repository.Setup(repo => repo.GetById(It.IsAny<User>(), It.IsAny<Guid>()))
            .ReturnsAsync(recipe);
        
        return this;
    }
    public RecipeRepositoryBuilder DeleteById(List<Recipe> recipes, Guid recipeId)
    {
        _repository.Setup(repo => repo.Delete(It.IsAny<Guid>()))
            .Returns((Guid id) =>
            {
                var recipe = recipes.AsQueryable().First(r => r.Id == id);
                recipes.Remove(recipe);
                return Task.CompletedTask;
            });
        
        return this;
    }
    
    public RecipeRepositoryBuilder Update(IMapper mapper,Recipe? recipeToUpdate, RequestRecipeJson newRecipe)
    {
        _repository.Setup(repo => repo.Update(It.IsAny<Recipe>()))
            .Callback((Recipe recipe) =>
            {
                mapper.Map(newRecipe, recipeToUpdate);
            });
        
        return this;
    }
    
    public RecipeRepositoryBuilder GetByUser(List<Recipe> recipes, int numberOfRecipes)
    {
        _repository.Setup(repo => repo.GetByUser(It.IsAny<User>(), It.IsAny<int>()))
            .ReturnsAsync((User user, int n) =>
            {
                return recipes
                    .OrderByDescending(r => r.CreatedOn)
                    .Take(numberOfRecipes)
                    .ToList();
            });
        
        return this;
    }

    public IRecipesRepository Build()
    {
        return _repository.Object;
    }
}