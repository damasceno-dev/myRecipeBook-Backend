using Moq;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Interfaces.OpenAI;

namespace CommonTestUtilities.Services;

public class ChatGptServiceBuilder
{
    private readonly Mock<IRecipeAIGenerator> _repository = new();

    public ChatGptServiceBuilder GenerateAIRecipe(RecipeDto recipe)
    {
        _repository
            .Setup(repo => repo.GenerateAIRecipe(It.IsAny<List<string>>()))
            .ReturnsAsync(() => recipe);
        return this;
    }
    
    public IRecipeAIGenerator Build()
    {
        return _repository.Object;
    }
}