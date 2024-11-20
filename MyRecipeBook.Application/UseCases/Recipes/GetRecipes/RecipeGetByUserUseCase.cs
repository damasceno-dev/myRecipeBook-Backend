using AutoMapper;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.GetRecipes;

public class RecipeGetByUserUseCase(IRecipesRepository recipesRepository, IUsersRepository usersRepository, IMapper mapper)
{ 
    public async Task<List<ResponseShortRecipeJson>> Execute(int numberOfRecipes)
    {
        Validate(numberOfRecipes);
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipes = await recipesRepository.GetByUser(user, numberOfRecipes);
        return recipes.Select(mapper.Map<ResponseShortRecipeJson>).ToList();
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