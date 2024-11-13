using CommonTestUtilities.Requests;
using MyRecipeBook.Application.Services;

namespace WebApi.Test.Token;

public class TokenTestHelper
{
    private readonly Dictionary<string, (string HttpMethod, Func<object> RequestBuilder)> _routeConfigurations =
        new()
        {
            { "user/changePassword", ("PUT", () => RequestUserChangePasswordJsonBuilder.Build(SharedValidators.MinimumPasswordLength + 1)) },
            { "user/update", ("PUT", RequestUserUpdateJsonBuilder.Build) },
            { "user/getProfileWithToken", ("GET", () => default!) },
        };

    public async Task<HttpResponseMessage> ExecuteRandomRoute(
        MyInMemoryFactory factory,
        string culture,
        string? token)
    {
        var randomRoute = _routeConfigurations.ElementAt(new Random().Next(_routeConfigurations.Count));
        var route = randomRoute.Key;
        var (httpMethod, requestBuilder) = randomRoute.Value;

        return httpMethod switch
        {
            "GET" => await factory.DoGet(route, culture: culture, token: token),
            "PUT" => await factory.DoPut(route, requestBuilder?.Invoke(), culture: culture, token: token),
            _ => throw new InvalidOperationException($"Unsupported HTTP method: {httpMethod}")
        };
    }
}