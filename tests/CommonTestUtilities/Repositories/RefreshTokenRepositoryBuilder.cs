using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace CommonTestUtilities.Repositories;

public class RefreshTokenRepositoryBuilder
{
    private readonly Mock<IRefreshTokenRepository> _repository;
    public RefreshTokenRepositoryBuilder()
    {
        _repository = new Mock<IRefreshTokenRepository>();
    }
    public RefreshTokenRepositoryBuilder Generate(string refreshToken)
    {
        _repository.Setup(rt => rt.Generate()).Returns(refreshToken);
        return this;
    }

    public RefreshTokenRepositoryBuilder GetRefreshToken(RefreshToken? refreshToken)
    {
        _repository.Setup(rt => rt.GetRefreshToken(It.IsAny<string>())).ReturnsAsync(refreshToken);
        return this;
    }
    
    public IRefreshTokenRepository Build()
    {
        return _repository.Object;
    }
}