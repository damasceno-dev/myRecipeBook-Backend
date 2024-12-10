using AutoMapper;
using Microsoft.Extensions.Options;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.Filter;

public class RecipeFilterUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IMapper mapper, IStorageService storageService)
{
    public async Task<List<ResponseShortRecipeJson>> Execute(RequestRecipeFilterJson requestRecipe)
    {
        Validate(requestRecipe);

        var user = await usersRepository.GetLoggedUserWithToken();
        var filter = mapper.Map<FilterRecipeDto>(requestRecipe);
        var recipesFiltered = await recipesRepository.FilterRecipe(user,filter);

        var response = new List<ResponseShortRecipeJson>(); 
        
        foreach (var recipe in recipesFiltered)
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

    private static void Validate(RequestRecipeFilterJson requestRecipe)
    {
        var result = new RecipeFilterFluentValidation().Validate(requestRecipe);
        if (result.IsValid is false)
        {
            var erros = result.Errors.Select(x => x.ErrorMessage).ToList();
            throw new OnValidationException(erros);
        }
    }
}