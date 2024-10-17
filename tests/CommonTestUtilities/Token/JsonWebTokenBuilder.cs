using MyRecipeBook.Infrastructure.Tokens;

namespace CommonTestUtilities.Token;

public class JsonWebTokenBuilder
{
    public static JsonWebTokenCreate Build()
    {
        const string signKey = "SymmetricSecurityKeyRequires32characterLongToMeetMinimalSizeForHMACSHA256";
        return new JsonWebTokenCreate(1000, signKey);
    }
}