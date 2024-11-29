using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using Xunit;

namespace WebApi.Test.Recipes.GenerateWithAI;

public class RecipeGenerateWithAIControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token:validToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.RootElement.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        result.RootElement.GetProperty("ingredients").EnumerateArray().Should().NotBeEmpty();
        result.RootElement.GetProperty("instructions").EnumerateArray().Should().NotBeEmpty();
        result.RootElement.GetProperty("dishTypes").EnumerateArray().Should().NotBeEmpty();
    }
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorIngredientListCountExceeded(string culture)
    {
        var numberOfIngredients = SharedValidators.MaximumRecipeIngredients + 5;
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build(numberOfIngredients);
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_LIST_MAXIMUM_COUNT", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token: validToken, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorIngredientEmpty(string culture)
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = string.Empty;
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_NOT_EMPTY", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token: validToken, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInvalidSeparators(string culture)
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1-cup-flour";
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_INVALID_SEPARATORS", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token: validToken, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInvalidStartCharacter(string culture)
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "-flour";
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_INVALID_START_CHARACTER", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token: validToken, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorMaximumWordCountExceeded(string culture)
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1 cup of very very very fine flour";
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_MAXIMUM_WORD_COUNT", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token: validToken, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInvalidCharacter(string culture)
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1 cup @ flour";
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_INVALID_CHARACTER", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);

        var response = await factory.DoPost("recipe/generateWithAI", request, token: validToken, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}