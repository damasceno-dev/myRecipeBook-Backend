using AutoMapper;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.Update;

public class RecipeUpdateUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IMapper mapper, IUnitOfWork unitOfWork)
{
    public async Task<ResponseRecipeJson> Execute(Guid recipeId, RequestRecipeJson newRecipe)
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = await recipesRepository.GetById(user,recipeId);
        Validate(newRecipe);
        ValidateExistingRecipe(recipe);
        
        mapper.Map(newRecipe, recipe);
        
        recipesRepository.Update(recipe!);
        await unitOfWork.Commit();
        
        return mapper.Map<ResponseRecipeJson>(recipe);
    }

    private static void ValidateExistingRecipe(Recipe? recipe)
    {
        if (recipe is null)
        {
            throw new NotFoundException(ResourceErrorMessages.RECIPE_NOT_FOUND);
        }
    }

    private static void Validate(RequestRecipeJson newRecipe)
    {
        var result = new RecipeRegisterAndUpdateFluentValidation().Validate(newRecipe);
        if (result.IsValid is false)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errors);
        }
    }
}