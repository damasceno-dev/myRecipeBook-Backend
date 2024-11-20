using CommonTestUtilities.Requests;
using MyRecipeBook.Application.Services;
using Xunit.Abstractions;

namespace WebApi.Test.Token;

public class TokenTestHelper(ITestOutputHelper output)
{
    private readonly Dictionary<string, (string HttpMethod, Func<object> RequestBuilder)> _routeConfigurations =
        new()
        {
            { "user/changePassword", ("PUT", () => RequestUserChangePasswordJsonBuilder.Build()) },
            { "user/update", ("PUT", RequestUserUpdateJsonBuilder.Build) },
            { "user/getProfileWithToken", ("GET", () => default!) },
            { "recipe/register", ("POST", RequestRecipeJsonBuilder.Build) },
            { "recipe/filter", ("POST", RequestRecipeFilterJsonBuilder.Build) },
            { $"recipe/getById/{Guid.NewGuid}", ("GET", () => default!) },
            { $"recipe/deleteById/{Guid.NewGuid}", ("DELETE", () => default!) },
            { $"recipe/update/{Guid.NewGuid}", ("PUT", () => default!) },
        };

    public async Task<IEnumerable<(string Route, HttpResponseMessage Response)>> ExecuteAllRoutes(
        MyInMemoryFactory factory,
        string culture,
        string? token)
    {
        var tasks = _routeConfigurations.Select(async routeConfig =>
        {
            var (route, (httpMethod, requestBuilder)) = routeConfig;
            output.WriteLine($"Executing route: {route} with HTTP method: {httpMethod} for culture: {culture}");

            var response = httpMethod switch
            {
                "GET" => await factory.DoGet(route, culture: culture, token: token),
                "DELETE" => await factory.DoDelete(route, culture: culture, token: token),
                "PUT" => await factory.DoPut(route, requestBuilder?.Invoke(), culture: culture, token: token),
                "POST" => await factory.DoPost(route, requestBuilder?.Invoke(), culture: culture, token: token),
                _ => throw new InvalidOperationException($"Unsupported HTTP method: {httpMethod}")
            };
            output.WriteLine($"Route: {route} executed with status: {response.StatusCode}");

            return (route, response);
        });

        return await Task.WhenAll(tasks);
    }
    
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
            "POST" => await factory.DoPost(route, requestBuilder?.Invoke(), culture: culture, token: token),
            _ => throw new InvalidOperationException($"Unsupported HTTP method: {httpMethod}")
        };
    }
}