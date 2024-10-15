using Bogus;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestUserLoginJsonBuilder
{
    public static RequestUserLoginJson Build()
    {
        return new Faker<RequestUserLoginJson>()
            .RuleFor(r => r.Email, f => f.Internet.Email())
            .RuleFor(r => r.Password, f => f.Internet.Password());
    }
}