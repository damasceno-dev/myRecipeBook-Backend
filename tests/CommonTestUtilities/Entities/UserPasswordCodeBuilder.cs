using MyRecipeBook.Domain.Entities;

namespace CommonTestUtilities.Entities;

public class UserPasswordCodeBuilder
{
    public static UserPasswordResetCode Build(Guid userId, string code)
    {
        return new UserPasswordResetCode { UserId = userId, Code = code };
    }
}