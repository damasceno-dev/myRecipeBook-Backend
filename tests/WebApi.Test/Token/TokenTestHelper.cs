using CommonTestUtilities.FormFile;
using CommonTestUtilities.Requests;
using Microsoft.AspNetCore.Http;
using MyRecipeBook.Communication.Requests;
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
            { "recipe/register", ("POST", () => RequestRecipeFormBuilder.Build())},
            { $"update/image/{Guid.NewGuid()}", ("PUT", () => FormFileBuilder.Build("sampleImage.jpg")) },
            { "recipe/filter", ("POST", RequestRecipeFilterJsonBuilder.Build) },
            { "recipe/generateWithAI", ("POST", () => RequestRecipeIngredientsForAIJsonBuilder.Build()) },
            { $"recipe/getById/{Guid.NewGuid()}", ("GET", () => default!) },
            { $"recipe/deleteById/{Guid.NewGuid()}", ("DELETE", () => default!) },
            { $"recipe/update/{Guid.NewGuid()}", ("PUT", RequestRecipeJsonBuilder.Build) },
            { $"recipe/getByUser/{new Random().Next(1,10)}", ("GET", () => default!) },
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

            var request = requestBuilder.Invoke();

            HttpResponseMessage response;
            if (request is RequestRecipeForm recipeForm)
            {
                response = await factory.DoPostRecipeForm(route, recipeForm, culture: culture, token: token);
            }
            else if (request is IFormFile file)
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
                response = await factory.DoPutMultipartForm(route, content, culture: culture, token: token);
            }
            else
            {
                response = httpMethod switch
                {
                    "GET" => await factory.DoGet(route, culture: culture, token: token),
                    "DELETE" => await factory.DoDelete(route, culture: culture, token: token),
                    "PUT" => await factory.DoPut(route, request, culture: culture, token: token),
                    "POST" => await factory.DoPost(route, request, culture: culture, token: token),
                    _ => throw new InvalidOperationException($"Unsupported HTTP method: {httpMethod}")
                };
            }

            output.WriteLine($"Route: {route} executed with status: {response.StatusCode}");
            return (route, response);
        });

        return await Task.WhenAll(tasks);
    }
}
