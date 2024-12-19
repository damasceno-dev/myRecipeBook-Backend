using Microsoft.AspNetCore.Http;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.ImageUpdateCover;

public class RecipeUpdateImageUseCase(IStorageService storageService, IRecipesRepository recipesRepository, IUsersRepository usersRepository, IUnitOfWork unitOfWork)
{
    public async Task Execute(IFormFile file, Guid recipeId)
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = await recipesRepository.GetById(user, recipeId);
        var fileStream = file.OpenReadStream();
        
        ValidateRecipe(recipe);
        var fileExtension = ValidateFileAndGetExtension(fileStream);

        recipe!.ImageIdentifier = $"{Guid.NewGuid()}.{fileExtension}";
        recipesRepository.Update(recipe);
        
        await unitOfWork.Commit();
        await storageService.Upload(user, fileStream, recipe.ImageIdentifier);
    }

    private static string ValidateFileAndGetExtension(Stream file)
    {
        var (isValidFile, extension) = file.ValidateImageAndGetExtension();
        
        if (isValidFile is false)
        {
            throw new OnValidationException([ResourceErrorMessages.IMAGE_INVALID_TYPE]);
        }

        return extension;
    }

    private static void ValidateRecipe(Recipe? recipe)
    {
        if (recipe is null)
        {
            throw new NotFoundException(ResourceErrorMessages.RECIPE_NOT_FOUND);
        }
    }
}