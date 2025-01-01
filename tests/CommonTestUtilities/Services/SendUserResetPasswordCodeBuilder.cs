using Moq;
using MyRecipeBook.Domain.Interfaces.Email;

namespace CommonTestUtilities.Services;

public class SendUserResetPasswordCodeBuilder
{
    public static ISendUserResetPasswordCode Build()
    {
        return new Mock<ISendUserResetPasswordCode>().Object;
    }
}