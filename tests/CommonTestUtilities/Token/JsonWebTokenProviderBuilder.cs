using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace CommonTestUtilities.Token;

public class JsonWebTokenProviderBuilder
{
    private readonly Mock<ITokenProvider> _repository;
    public JsonWebTokenProviderBuilder()
    {
        _repository = new Mock<ITokenProvider>();
    }
    public ITokenProvider Build()
    {
        _repository.Setup(u => u.Value()).Returns("validToken");
        return _repository.Object;
    }
}