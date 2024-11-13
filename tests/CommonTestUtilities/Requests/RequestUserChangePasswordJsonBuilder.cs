using Bogus;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestUserChangePasswordJsonBuilder
{
    private const int MinPasswordLength = SharedValidators.MinimumPasswordLength + 1;
    public static RequestUserChangePasswordJson Build(int passwordLength = MinPasswordLength)
    {
        return new Faker<RequestUserChangePasswordJson>()
            .RuleFor(u => u.CurrentPassword,  f => f.Internet.Password(passwordLength))
            .RuleFor(u => u.NewPassword,  f => f.Internet.Password(passwordLength));
    }
}