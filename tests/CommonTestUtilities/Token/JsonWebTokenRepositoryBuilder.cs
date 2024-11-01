using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Infrastructure.Tokens;

namespace CommonTestUtilities.Token;

public class JsonWebTokenRepositoryBuilder
{
    const string SignKey = "SymmetricSecurityKeyRequires32characterLongToMeetMinimalSizeForHMACSHA256";
    public static JsonWebTokenRepository Build()
    {
        return new JsonWebTokenRepository(1000, SignKey);
    }
    public static JsonWebTokenRepository BuildExpiredToken()
    {
        return new JsonWebTokenRepository(0.001, SignKey);
    }
}