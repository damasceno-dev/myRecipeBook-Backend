using Bogus;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestUserUpdateJsonBuilder
{
    public static RequestUserUpdateJson Build()
    {
        return new Faker<RequestUserUpdateJson>()
            .RuleFor(u => u.Name, f => f.Person.FirstName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(null, u.Name));
    }
}