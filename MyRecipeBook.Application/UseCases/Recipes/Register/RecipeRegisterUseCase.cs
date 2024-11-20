using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.Register;

public class RecipeRegisterUseCase(IMapper mapper, IUnitOfWork unitOfWork, IUsersRepository usersRepository, IRecipesRepository recipeRepository)
{
    public async Task<ResponseRegisteredRecipeJson> Execute(RequestRecipeJson request)
    {
        Validate(request);
        
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = mapper.Map<Recipe>(request);
        recipe.UserId = user.Id;
        
        await recipeRepository.Register(recipe);
        await unitOfWork.Commit();
        
        return mapper.Map<ResponseRegisteredRecipeJson>(recipe);
    }

    private static void Validate(RequestRecipeJson request)
    {
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(request);
        if (result.IsValid is false)
        {
            var erros = result.Errors.Select(x => x.ErrorMessage).ToList();
            throw new OnValidationException(erros);
        }
    }
    
    
}