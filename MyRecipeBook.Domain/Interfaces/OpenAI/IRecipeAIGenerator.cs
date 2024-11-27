using MyRecipeBook.Domain.Dtos;

namespace MyRecipeBook.Domain.Interfaces.OpenAI;

public interface IRecipeAIGenerator
{
    Task<RecipeDto> GenerateAIRecipe(IList<string> ingredients);
}