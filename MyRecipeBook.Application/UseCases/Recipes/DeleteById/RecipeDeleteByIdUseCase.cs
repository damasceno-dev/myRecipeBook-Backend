using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.DeleteById;

public class RecipeDeleteByIdUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IUnitOfWork unitOfWork)
{
    public async Task Execute(Guid recipeId)
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = await recipesRepository.GetByIdAsNoTracking(user, recipeId);
        Validate(recipe);
        await recipesRepository.Delete(recipeId);
        await unitOfWork.Commit();
    }

    private void Validate(Recipe? recipe)
    {
        if (recipe is null)
        {
            throw new NotFoundException(ResourceErrorMessages.RECIPE_NOT_FOUND);
        }
    }
}