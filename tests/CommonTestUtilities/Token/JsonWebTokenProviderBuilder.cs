using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace CommonTestUtilities.Token;

public class JsonWebTokenProviderBuilder
{
    public static ITokenProvider Build()
    {
        var mock = new Mock<ITokenProvider>();
        mock.Setup(u => u.Value()).Returns("validToken");
        return mock.Object;
    }
}