using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Token;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using Xunit;

namespace WebApi.Test.Recipes.GetByUsers;

public class RecipeGetByUsersControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var numberOfRecipes = new Random().Next(1, factory.GetRecipes().Count);
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoGet($"recipe/getByUser/{numberOfRecipes}", token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetArrayLength().Should().Be(numberOfRecipes);
        var expectedRecipes = factory.GetRecipes()
            .Where(r => r.UserId == factory.GetUser().Id && r.Active)
            .OrderByDescending(r => r.CreatedOn)
            .Take(numberOfRecipes)
            .ToList();
        for (var i = 0; i < numberOfRecipes; i++)
        {
            var recipeFromResponse = result.RootElement[i];
            var expectedRecipe = expectedRecipes[i];

            recipeFromResponse.GetProperty("id").GetGuid().Should().Be(expectedRecipe.Id);
            recipeFromResponse.GetProperty("title").GetString().Should().Be(expectedRecipe.Title);
            recipeFromResponse.GetProperty("quantityIngredients").GetInt32().Should().Be(expectedRecipe.Ingredients.Count);
        }
    }  
    
    [Fact]
    public async Task SuccessFromJsonSerializeInMemory()
    {
        var user = factory.GetUser();
        var numberOfRecipes = new Random().Next(1, factory.GetRecipes().Count);
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(user.Id);

        var response = await factory.DoGet($"recipe/getByUser/{numberOfRecipes}", token: validToken);
        var recipesFromResponse = await response.Content.ReadFromJsonAsync<List<ResponseShortRecipeJson>>();

        var recipesInDb = await factory.GetDbContext()
            .Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedOn)
            .Take(numberOfRecipes)
            .ToListAsync();

        recipesFromResponse.Should().NotBeNull();
        recipesFromResponse!.Count.Should().Be(numberOfRecipes);
        for (var i = 0; i < numberOfRecipes; i++)
        {
            var recipeFromResponse = recipesFromResponse[i];
            var expectedRecipe = recipesInDb[i];

            recipeFromResponse.Id.Should().Be(expectedRecipe.Id);
            recipeFromResponse.Title.Should().Be(expectedRecipe.Title);
            recipeFromResponse.QuantityIngredients.Should().Be(expectedRecipe.Ingredients.Count);
        }
    }
    
        
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorNotFoundRecipe(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_NUMBER_GREATER_THAN_0", new CultureInfo(culture));
        var numberOfRecipes = new Random().Next(-100,0);
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoGet($"recipe/getByUser/{numberOfRecipes}", token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}