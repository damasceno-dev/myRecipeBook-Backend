using MyRecipeBook.Infrastructure.Tokens;

namespace CommonTestUtilities.Token;

public class JsonWebTokenBuilder
{
    public static JsonWebTokenRepository Build()
    {
        const string signKey = "SymmetricSecurityKeyRequires32characterLongToMeetMinimalSizeForHMACSHA256";
        return new JsonWebTokenRepository(1000, signKey);
    }
}