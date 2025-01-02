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
    public async Task<ResponseRecipeJson> Execute(RequestRecipeForm request)
    {
        Validate(request);
        
        var user = await usersRepository.GetLoggedUserWithToken();
        var recipe = mapper.Map<Recipe>(request);
        recipe.UserId = user.Id;

        var response = mapper.Map<ResponseRecipeJson>(recipe);
        
        if (request.ImageFile is not null)
        {
            var fileStream = request.ImageFile.OpenReadStream();
            var fileExtension = ValidateFileAndGetExtension(fileStream);
            recipe.ImageIdentifier = $"{Guid.NewGuid()}.{fileExtension}";
            await storageService.Upload(user, fileStream, recipe.ImageIdentifier);
            var imageUrl = await storageService.GetFileUrl(user, recipe.ImageIdentifier);
            response.ImageUrl = imageUrl;
        }
        
        await recipeRepository.Register(recipe);
        await unitOfWork.Commit();
        
        return response;
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