using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Enums;
using Xunit;

namespace WebApi.Test.Recipes.Filter;

public class RecipeFilterControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var request = RequestRecipeFilterJsonBuilder.BuildFromList(factory.GetRecipes());
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoPost("recipe/filter", request, token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        var expectedRecipe = factory.GetRecipes()
            .First(r => r.Title.Equals(request.TitleIngredient));
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.EnumerateArray().Should().ContainSingle(e =>
                e.GetProperty("title").GetString()!.Equals(request.TitleIngredient, StringComparison.OrdinalIgnoreCase) &&
                e.GetProperty("quantityIngredients").GetInt32() == expectedRecipe.Ingredients.Count);
    }
    
    [Fact]
    public async Task SuccessFromJsonSerializeInMemory()
    {
        var request = RequestRecipeFilterJsonBuilder.BuildFromList(factory.GetRecipes());
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoPost("recipe/filter", request, token: validToken);
        var recipesFromJson = await response.Content.ReadFromJsonAsync<List<ResponseShortRecipeJson>>();
        var recipeFromJson = recipesFromJson!.First();
        var expectedRecipe = factory.GetRecipes().First(r => r.Title.Equals(request.TitleIngredient));
        var recipeInDb = await factory.GetDbContext().Recipes.Include(recipe => recipe.Ingredients).SingleAsync(u => u.Id == recipeFromJson!.Id);
        
        recipeInDb.Should().NotBeNull();
        recipeInDb!.Title.Should().Be(request.TitleIngredient);
        recipeInDb!.Id.Should().NotBeEmpty();
        recipeInDb!.Ingredients.Count().Should().Be(expectedRecipe.Ingredients.Count);
    }
    
    [Fact]
    public async Task SuccessNoContentFromResponseBodyContainer()
    {
        var request = RequestRecipeFilterJsonBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        
        var response = await factory.DoPost("recipe/filter", request, token: validToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorRecipeFilterNotInEnum(string culture)
    {
        var notInEnumDishType = ResourceErrorMessages.ResourceManager.GetString("RECIPE_DISH_TYPE_NOT_IN_ENUM", new CultureInfo(culture));
        var notInEnumDifficulty = ResourceErrorMessages.ResourceManager.GetString("RECIPE_DIFFICULTY_NOT_IN_ENUM", new CultureInfo(culture));
        var notInEnumCookingTime = ResourceErrorMessages.ResourceManager.GetString("RECIPE_COOKING_TIME_NOT_IN_ENUM", new CultureInfo(culture));
        List<string?> expectedErrorMessages = [notInEnumDishType, notInEnumDifficulty, notInEnumCookingTime ];
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        var request = RequestRecipeFilterJsonBuilder.Build();
        request.DishTypes.Add((DishType)EnumTestHelper.OutOfRangeEnum<DishType>());
        request.Difficulties.Add((Difficulty)EnumTestHelper.OutOfRangeEnum<Difficulty>());
        request.CookingTimes.Add((CookingTime)EnumTestHelper.OutOfRangeEnum<CookingTime>());
        
        var response = await factory.DoPost("recipe/filter", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .Contain(e => expectedErrorMessages.Contains(e.GetString()));
    }   
}