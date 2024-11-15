using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Dtos;
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

    public async Task<List<Recipe>> FilterRecipe(User user,FilterRecipeDto filter)
    {
        var recipes = _dbContext
            .Recipes
            .AsNoTracking()
            .Include(recipe => recipe.Ingredients)
            .Where(recipe => recipe.Active && recipe.UserId == user.Id);

        recipes = RecipeFilterLogic(recipes, filter);

        return await recipes.ToListAsync();
    }

    public static IQueryable<Recipe> RecipeFilterLogic(IQueryable<Recipe> recipes, FilterRecipeDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.TitleIngredient))
        {
            string titleIngredientLower = filter.TitleIngredient.ToLower();
            recipes = recipes.Where(recipe =>
                recipe.Title.ToLower().Contains(titleIngredientLower) ||
                recipe.Ingredients.Any(i => i.Item.ToLower().Contains(titleIngredientLower)));
        }
        if (filter.Difficulties.Any())
        {
            recipes = recipes.Where(recipe =>
                recipe.Difficulty.HasValue && filter.Difficulties.Contains(recipe.Difficulty.Value));
        }
        if (filter.CookingTimes.Any())
        {
            recipes = recipes.Where(recipe =>
                recipe.CookingTime.HasValue && filter.CookingTimes.Contains(recipe.CookingTime.Value));
        }
        if (filter.DishTypes.Any())
        {
            recipes = recipes.Where(recipe =>
                recipe.DishTypes.Any(dishType => filter.DishTypes.Contains(dishType.Type)));
        }

        return recipes;
    }
}