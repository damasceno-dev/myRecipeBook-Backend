using System.Text.Json;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Interfaces.OpenAI;
using OpenAI.Chat;

namespace MyRecipeBook.Infrastructure.Services;

public class ChatGptService(ChatClient chatGpTclient): IRecipeAIGenerator
{

    public const string ChatModel = "gpt-4o";
    private static readonly JsonSerializerOptions PropertyNameCaseInsensitiveOption = new() { PropertyNameCaseInsensitive = true };
    public async Task<RecipeDto> GenerateAIRecipe(IList<string> ingredients)
    {
        var recipeGenerationCommand = ResourceOpenAI.RECIPE_GENERATOR_STARTING_MESSAGE;
        recipeGenerationCommand = recipeGenerationCommand.Replace("{ingredientsList}", string.Join(", ", ingredients));

        try
        {
            var response = await chatGpTclient.CompleteChatAsync(new SystemChatMessage(recipeGenerationCommand));
            var jsonResponse = CleanResponse(response.Value.Content[0].Text);
            
            var recipe = JsonSerializer.Deserialize<RecipeDto>(jsonResponse, PropertyNameCaseInsensitiveOption);
            if (recipe is null) throw new InvalidOperationException("Failed to parse recipe from Json chatgpt response");
            return recipe;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Error parsing recipe JSON", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unknown error", ex);
        }
    }

    private static string CleanResponse(string jsonResponse)
    {
        var cleanedResponse = jsonResponse.Trim();

        if (cleanedResponse.StartsWith("```json"))
        {
            cleanedResponse = cleanedResponse[7..];
        }

        if (cleanedResponse.EndsWith("```"))
        {
            cleanedResponse = cleanedResponse[..^3];
        }

        return cleanedResponse;
    }
}