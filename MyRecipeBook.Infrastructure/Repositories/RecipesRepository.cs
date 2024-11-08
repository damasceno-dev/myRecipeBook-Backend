using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Repositories;

public class RecipesRepository : IRecipesRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public RecipesRepository(MyRecipeBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Register(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
    }
}