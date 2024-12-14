using AutoMapper;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.Register;

public class RecipeRegisterUseCase(IMapper mapper, IUnitOfWork unitOfWork, IUsersRepository usersRepository, IRecipesRepository recipeRepository, IStorageService storageService)
{
    public async Task<ResponseRegisteredRecipeJson> Execute(RequestRecipeForm request)
    {
        Validate(request);
        
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = mapper.Map<Recipe>(request);
        recipe.UserId = user.Id;

        if (request.ImageFile is not null)
        {
            var fileStream = request.ImageFile.OpenReadStream();
            var fileExtension = ValidateFileAndGetExtension(fileStream);
            recipe.ImageIdentifier = $"{Guid.NewGuid()}.{fileExtension}";
            await storageService.Upload(user, fileStream, recipe.ImageIdentifier);
        }
        
        await recipeRepository.Register(recipe);
        await unitOfWork.Commit();
        
        return mapper.Map<ResponseRegisteredRecipeJson>(recipe);
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