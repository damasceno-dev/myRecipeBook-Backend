using Bogus;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestUserChangePasswordJsonBuilder
{
    public static RequestUserChangePasswordJson Build(int passwordLength)
    {
        return new Faker<RequestUserChangePasswordJson>()
            .RuleFor(u => u.CurrentPassword,  f => f.Internet.Password(passwordLength))
            .RuleFor(u => u.NewPassword,  f => f.Internet.Password(passwordLength));
    }
}