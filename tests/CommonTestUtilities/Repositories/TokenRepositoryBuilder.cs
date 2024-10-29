using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace CommonTestUtilities.Repositories;

public class TokenRepositoryBuilder
{
    public static ITokenRepository Build()
    {
        var tokenRepositoryMock = new Mock<ITokenRepository>();

        tokenRepositoryMock
            .Setup(tr => tr.ValidateAndGetUserIdentifier(It.IsAny<string>()))
            .Returns(Guid.NewGuid());

        return tokenRepositoryMock.Object;
    }
}