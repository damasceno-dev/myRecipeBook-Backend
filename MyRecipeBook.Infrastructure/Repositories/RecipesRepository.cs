using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Repositories;

public class RecipesRepository(MyRecipeBookDbContext dbContext): IRecipesRepository
{
    public async Task Register(Recipe recipe)
    {
        await dbContext.Recipes.AddAsync(recipe);
    }

    public async Task<List<Recipe>> FilterRecipe(User user,FilterRecipeDto filter)
    {
        var recipes = dbContext
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
            var titleIngredientLower = filter.TitleIngredient.ToLower();
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
    
    public async Task<Recipe?> GetByIdAsNoTracking(User user,Guid id)
    {
        return await GetRecipeByIdLogic()
            .AsNoTracking()
            .Where(recipe => recipe.Id == id && recipe.UserId == user.Id)
            .FirstOrDefaultAsync(recipe => recipe.Id == id);
    }    
    
    public async Task<Recipe?> GetById(User user,Guid id)
    {
        return await GetRecipeByIdLogic()
            .Where(recipe => recipe.Id == id && recipe.UserId == user.Id)
            .FirstOrDefaultAsync(recipe => recipe.Id == id);
    }

    public async Task Delete(Guid id)
    {
        var recipe = await dbContext.Recipes.FirstAsync(r => r.Id == id);
        dbContext.Recipes.Remove(recipe);
    }

    public void Update(Recipe recipe)
    {
        dbContext.Recipes.Update(recipe);
    }

    private IIncludableQueryable<Recipe, IList<DishType>> GetRecipeByIdLogic()
    {
        return dbContext.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Instructions)
            .Include(r => r.DishTypes);
    }
}