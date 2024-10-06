using Bogus;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestUserRegisterJsonBuilder
{
    public static RequestUserRegisterJson Build()
    {
        return new Faker<RequestUserRegisterJson>()
            .RuleFor(u => u.Name, f => f.Person.FirstName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(null, u.Name))
            .RuleFor(u => u.Password, f => f.Internet.Password());
    }
}