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
            .ReturnsAsync(() =>
            {
                var filteredRecipes = RecipesRepository.RecipeFilterLogic(recipes.AsQueryable(), filterDto);
                return filteredRecipes.ToList();
            });

        return this;
    }

    public IRecipesRepository Build()
    {
        return _repository.Object;
    }
}