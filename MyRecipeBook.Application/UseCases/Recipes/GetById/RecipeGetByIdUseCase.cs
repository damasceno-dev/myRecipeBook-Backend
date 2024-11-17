using AutoMapper;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.GetById;

public class RecipeGetByIdUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IMapper mapper)
{
    public async Task<ResponseRecipeJson> Execute(Guid id)
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = await recipesRepository.GetById(user, id);
        Validate(recipe);
        return mapper.Map<ResponseRecipeJson>(recipe);
    }

    private static void Validate(Recipe? recipe)
    {
        if (recipe is null)
        {
            throw new NotFoundException(ResourceErrorMessages.RECIPE_NOT_FOUND);
        }
    }
}