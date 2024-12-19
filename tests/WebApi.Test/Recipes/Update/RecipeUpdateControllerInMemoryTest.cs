using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Communication;
using Xunit;

namespace WebApi.Test.Recipes.Update;

public class RecipeUpdateControllerInMemoryTest(MyInMemoryFactory factory): IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var recipeToUpdateId = factory.GetRecipes().First().Id;
        var request = RequestRecipeJsonBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoPut($"recipe/update/{recipeToUpdateId}", request, token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("title").GetString().Should().Be(request.Title);
        result.RootElement.GetProperty("cookingTime").GetInt32().Should().Be((int)request.CookingTime!);
        result.RootElement.GetProperty("difficulty").GetInt32().Should().Be((int)request.Difficulty!);
        result.RootElement.GetProperty("ingredients").EnumerateArray()
            .Select(e => e.GetString())
            .Should().BeEquivalentTo(request.Ingredients);
        result.RootElement.GetProperty("instructions").EnumerateArray()
            .Select(e => e.GetProperty("text").GetString())
            .Should().BeEquivalentTo(request.Instructions.Select(i => i.Text));
        result.RootElement.GetProperty("dishTypes").EnumerateArray()
            .Select(e => e.GetInt32())
            .Should().BeEquivalentTo(request.DishTypes.Cast<int>());
    }
        
    [Fact]
    public async Task SuccessFromJsonSerializeInMemory()
    {
        var recipeToUpdateId = factory.GetRecipes().First().Id;
        var request = RequestRecipeJsonBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        await factory.DoPut($"recipe/update/{recipeToUpdateId}", request, token: validToken);
        var recipeInDb = await factory.GetDbContext()
            .Recipes
            .AsNoTracking()
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Instructions)
            .Include(recipe => recipe.DishTypes)
            .SingleAsync(r => r.Id == recipeToUpdateId);
        
        recipeInDb.Should().NotBeNull();
        recipeInDb!.Id.Should().Be(recipeToUpdateId);
        recipeInDb.Title.Should().Be(request.Title);
        recipeInDb.CookingTime.Should().Be(request.CookingTime);
        recipeInDb.Difficulty.Should().Be(request.Difficulty);
        recipeInDb.Ingredients.Select(i => i.Item).Should().BeEquivalentTo(request.Ingredients);
        recipeInDb.Instructions.Select(i => i.Text).Should().BeEquivalentTo(request.Instructions.Select(i => i.Text));
        recipeInDb.DishTypes.Select(d => d.Type).Should().BeEquivalentTo(request.DishTypes);
    }
        
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorRecipeNotFound(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_NOT_FOUND", new CultureInfo(culture));
        var recipeToUpdateId = Guid.NewGuid();
        var request = RequestRecipeJsonBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoPut($"recipe/update/{recipeToUpdateId}", request, token: validToken, culture: culture);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }       
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorRecipeTitleEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_TITLE_EMPTY", new CultureInfo(culture));
        var recipeToUpdateId = factory.GetRecipes().First().Id;
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoPut($"recipe/update/{recipeToUpdateId}", request, token: validToken, culture: culture);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}