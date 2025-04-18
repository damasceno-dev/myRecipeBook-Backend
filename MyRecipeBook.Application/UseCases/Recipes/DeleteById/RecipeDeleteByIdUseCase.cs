using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.DeleteById;

public class RecipeDeleteByIdUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IUnitOfWork unitOfWork, IStorageService storageService)
{
    public async Task Execute(Guid recipeId)
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = await recipesRepository.GetByIdAsNoTracking(user, recipeId);
        Validate(recipe);

        if (string.IsNullOrWhiteSpace(recipe!.ImageIdentifier) is false)
        {
            await storageService.Delete(user, recipe.ImageIdentifier);
        }
        
        await recipesRepository.Delete(recipeId);
        await unitOfWork.Commit();
    }

    private static void Validate(Recipe? recipe)
    {
        if (recipe is null)
        {
            throw new NotFoundException(ResourceErrorMessages.RECIPE_NOT_FOUND);
        }
    }
}