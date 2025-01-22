using MyRecipeBook.Application.Services;
using MyRecipeBook.Domain.Entities;

namespace CommonTestUtilities.Entities;

public class RefreshTokenBuilder
{
    private const int ExpiredDays = SharedValidators.RefreshTokenExpirationTimeInDays + 1;
    private static User User => UserBuilder.Build().user;
    public static string GenerateTestRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    public static RefreshToken Build(User? user = null, bool expired = false)
    {
        user ??= User;
        return new RefreshToken
        {
            UserId = user.Id,
            Value = GenerateTestRefreshToken(),
            CreatedOn = expired ? DateTime.UtcNow.AddDays(-ExpiredDays) : DateTime.UtcNow,
        };
    }
}