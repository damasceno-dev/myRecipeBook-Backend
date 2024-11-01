using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace CommonTestUtilities.Repositories;

public class TokenRepositoryBuilder
{
    private readonly Mock<ITokenRepository> _repository;
    public TokenRepositoryBuilder()
    {
        _repository = new Mock<ITokenRepository>();
    }
    public TokenRepositoryBuilder ValidateAndGetUserIdentifier(Guid id)
    {
        _repository
            .Setup(tr => tr.ValidateAndGetUserIdentifier(It.IsAny<string>()))
            .Returns(id);

        return this;
    }

    public ITokenRepository Build()
    {
        return _repository.Object;
    }
}