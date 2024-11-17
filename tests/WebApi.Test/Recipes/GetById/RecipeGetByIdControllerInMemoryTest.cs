using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Communication;
using Xunit;

namespace WebApi.Test.Recipes.GetById;

public class RecipeGetByIdControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var recipeId = factory.GetRecipes().First().Id;
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoGet($"recipe/getById/{recipeId}", token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.RootElement.GetProperty("id").GetString().Should().Be(recipeId.ToString());
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorNotFoundRecipe(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_NOT_FOUND", new CultureInfo(culture));
        var recipeId = Guid.NewGuid();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoGet($"recipe/getById/{recipeId}", token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}