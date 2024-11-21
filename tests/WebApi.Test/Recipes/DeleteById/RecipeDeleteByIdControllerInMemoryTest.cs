using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Communication;
using Xunit;

namespace WebApi.Test.Recipes.DeleteById;

public class RecipeDeleteByIdControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var recipeId = factory.GetRecipes().First().Id;
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoDelete($"recipe/deleteById/{recipeId}", token: validToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        factory.GetDbContext().Recipes.FirstOrDefault(r => r.Id == recipeId).Should().BeNull();
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorNotFoundRecipe(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_NOT_FOUND", new CultureInfo(culture));
        var recipeId = Guid.NewGuid();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoDelete($"recipe/deleteById/{recipeId}", token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}