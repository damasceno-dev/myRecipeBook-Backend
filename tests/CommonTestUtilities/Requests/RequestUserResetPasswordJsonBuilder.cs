using Bogus;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestUserResetPasswordJsonBuilder
{
    public static RequestUserResetPasswordJson Build(int passwordLength = 10)
    {
        return new Faker<RequestUserResetPasswordJson>()
            .RuleFor(u => u.Code, DigitGenerator.Generate6DigitCode)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email())
            .RuleFor(u => u.NewPassword, f => f.Internet.Password(passwordLength));
    }
}