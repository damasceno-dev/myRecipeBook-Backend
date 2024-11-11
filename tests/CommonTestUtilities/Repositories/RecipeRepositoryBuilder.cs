using Moq;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Repositories;

public class RecipeRepositoryBuilder
{
    private readonly Mock<IRecipesRepository> _repository;

    public RecipeRepositoryBuilder()
    {
        _repository = new Mock<IRecipesRepository>();
    }

    public IRecipesRepository Build()
    {
        return _repository.Object;
    }
}