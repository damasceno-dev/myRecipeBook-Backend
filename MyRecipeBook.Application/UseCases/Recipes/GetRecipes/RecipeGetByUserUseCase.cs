using AutoMapper;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.GetRecipes;

public class RecipeGetByUserUseCase(IRecipesRepository recipesRepository, IUsersRepository usersRepository, IMapper mapper, IStorageService storageService)
{ 
    public async Task<List<ResponseShortRecipeJson>> Execute(int numberOfRecipes)
    {
        Validate(numberOfRecipes);
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipes = await recipesRepository.GetByUser(user, numberOfRecipes);
        
        var response = new List<ResponseShortRecipeJson>(); 
        
        foreach (var recipe in recipes)
        {
            var shortRecipeJson = mapper.Map<ResponseShortRecipeJson>(recipe);
            if (string.IsNullOrWhiteSpace(recipe.ImageIdentifier) is false)
            {
                var imageUrl = await storageService.GetFileUrl(user, recipe.ImageIdentifier);
                shortRecipeJson.ImageUrl = imageUrl;
            }
            response.Add(shortRecipeJson);
        }

        return response;
    }

    private static void Validate(int numberOfRecipes)
    {
        var result = new RecipeGetByUserFluentValidation().Validate(numberOfRecipes);
        if (result.IsValid is false)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errors);
        }
    }
}