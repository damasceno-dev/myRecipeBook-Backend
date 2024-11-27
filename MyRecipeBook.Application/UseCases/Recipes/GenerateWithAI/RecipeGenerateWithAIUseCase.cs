using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.OpenAI;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;

public class RecipeGenerateWithAIUseCase(IRecipeAIGenerator recipeAIGenerator, IMapper mapper)
{
    public async Task<ResponseRecipeGeneratedJson> Execute(RequestRecipeIngredientsForAIJson request)
    {
        Validate(request);
        var newRecipeGenerated = await recipeAIGenerator.GenerateAIRecipe(request.Ingredients);
        
        return mapper.Map<ResponseRecipeGeneratedJson>(newRecipeGenerated);
    }

    private static void Validate(RequestRecipeIngredientsForAIJson request)
    {
        var result = new RecipeGenerateWithAIFluentValidation().Validate(request);
        if (result.IsValid is false)
        {
            var errorMessages = result.Errors.Select(v => v.ErrorMessage).ToList();
            throw new OnValidationException(errorMessages);
        }
    }
}