using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Infrastructure.Tokens;

namespace CommonTestUtilities.Token;

public class JsonWebTokenRepositoryBuilder
{
    public static JsonWebTokenRepository Build()
    {
        const string signKey = "SymmetricSecurityKeyRequires32characterLongToMeetMinimalSizeForHMACSHA256";
        return new JsonWebTokenRepository(1000, signKey);
    }
    public static JsonWebTokenRepository BuildExpiredToken()
    {
        const string signKey = "SymmetricSecurityKeyRequires32characterLongToMeetMinimalSizeForHMACSHA256";
        return new JsonWebTokenRepository(0.001, signKey);
    }
}