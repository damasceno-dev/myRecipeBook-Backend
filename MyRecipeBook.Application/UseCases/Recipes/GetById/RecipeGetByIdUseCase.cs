using AutoMapper;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.GetById;

public class RecipeGetByIdUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IMapper mapper, IStorageService storageService)
{
    public async Task<ResponseRecipeJson> Execute(Guid id)
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = await recipesRepository.GetByIdAsNoTracking(user, id);
        Validate(recipe);
        
        var response = mapper.Map<ResponseRecipeJson>(recipe);
        
        if (string.IsNullOrWhiteSpace(recipe!.ImageIdentifier) is false)
        {
            var imageUrl = await storageService.GetFileUrl(user, recipe.ImageIdentifier);
            response.ImageUrl = imageUrl;
        }
        
        return response;
    }

    private static void Validate(Recipe? recipe)
    {
        if (recipe is null)
        {
            throw new NotFoundException(ResourceErrorMessages.RECIPE_NOT_FOUND);
        }
    }
}