using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IRecipesRepository
{
    Task Register(Recipe recipe);
    Task<List<Recipe>> FilterRecipe(User user, FilterRecipeDto filter);
    Task<Recipe?> GetByIdAsNoTracking(User user, Guid id);
    Task<Recipe?> GetById(User user, Guid id);
    Task<List<Recipe>> GetByUser(User user, int numberOfRecipes);
    Task Delete(Guid id);
    void Update(Recipe recipe);
}