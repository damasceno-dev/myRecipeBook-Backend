using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IRecipesRepository
{
    Task Register(Recipe recipe);
    Task<List<Recipe>> FilterRecipe(User user, FilterRecipeDto filter);
    Task<Recipe?> GetById(User user, Guid id);
    Task Delete(Guid id);
}