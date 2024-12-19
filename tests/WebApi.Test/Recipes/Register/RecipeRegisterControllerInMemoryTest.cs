using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Amazon.Runtime.Internal.Util;
using CommonTestUtilities.FormFile;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Enums;
using Xunit;

namespace WebApi.Test.Recipes.Register;

public class RecipeRegisterControllerInMemoryTest(MyInMemoryFactory inMemoryFactory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    public async Task SuccessFromResponseBodyContainerNoImage()
    {
        var request = RequestRecipeFormBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.RootElement.GetProperty("title").GetString().Should().Be(request.Title);
        result.RootElement.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.RootElement.GetProperty("cookingTime").GetInt32().Should().Be((int)request.CookingTime!);
        result.RootElement.GetProperty("difficulty").GetInt32().Should().Be((int)request.Difficulty!);
        result.RootElement.GetProperty("ingredients").EnumerateArray()
            .Select(e => e.GetString())
            .Should().BeEquivalentTo(request.Ingredients);
        result.RootElement.GetProperty("instructions").EnumerateArray()
            .Select(e => new 
            {
                Step = e.GetProperty("step").GetInt32(),
                Text = e.GetProperty("text").GetString()
            })
            .Should().BeEquivalentTo(request.Instructions.Select(i => new 
            {
                i.Step,
                i.Text
            }));
        result.RootElement.GetProperty("dishTypes").EnumerateArray()
            .Select(e => e.GetInt32())
            .Should().BeEquivalentTo(request.DishTypes.Cast<int>());
        result.RootElement.TryGetProperty("imageUrl", out var imageUrl).Should().BeTrue();
        imageUrl.GetString().Should().BeNullOrEmpty();
    }
    
    [Theory]
    [InlineData("jpgImage.jpg")]
    [InlineData("pngImage.png")]
    public async Task SuccessFromResponseBodyContainerWithImage(string imageName)
    {
        var file = FormFileBuilder.Build(imageName);
        var request = RequestRecipeFormBuilder.Build(file);
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.RootElement.GetProperty("title").GetString().Should().Be(request.Title);
        result.RootElement.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.RootElement.GetProperty("cookingTime").GetInt32().Should().Be((int)request.CookingTime!);
        result.RootElement.GetProperty("difficulty").GetInt32().Should().Be((int)request.Difficulty!);
        result.RootElement.GetProperty("ingredients").EnumerateArray()
            .Select(e => e.GetString())
            .Should().BeEquivalentTo(request.Ingredients);
        result.RootElement.GetProperty("instructions").EnumerateArray()
            .Select(e => new 
            {
                Step = e.GetProperty("step").GetInt32(),
                Text = e.GetProperty("text").GetString()
            })
            .Should().BeEquivalentTo(request.Instructions.Select(i => new 
            {
                i.Step,
                i.Text
            }));
        result.RootElement.GetProperty("dishTypes").EnumerateArray()
            .Select(e => e.GetInt32())
            .Should().BeEquivalentTo(request.DishTypes.Cast<int>());
        result.RootElement.GetProperty("imageUrl").GetString().Should().NotBeNullOrEmpty();
        result.RootElement.GetProperty("imageUrl").GetString().Should().EndWith(Path.GetExtension(imageName));
    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeInMemory()
    {
        var request = RequestRecipeFormBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken);
        var recipeFromJson = await response.Content.ReadFromJsonAsync<ResponseRecipeJson>();
        var recipeInDb = await inMemoryFactory.GetDbContext().Recipes.SingleAsync(u => u.Id == recipeFromJson!.Id);
        
        recipeInDb.Should().NotBeNull();
        recipeInDb!.Title.Should().Be(request.Title);
        recipeInDb!.Id.Should().NotBeEmpty();
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInvalidFileType(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("IMAGE_INVALID_TYPE", new CultureInfo(culture));
        var file = FormFileBuilder.Build("textFile.txt");
        var request = RequestRecipeFormBuilder.Build(file);
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture:culture);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorTitleEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_TITLE_EMPTY", new CultureInfo(culture));
        var request = RequestRecipeFormBuilder.Build();
        request.Title = string.Empty;
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    

    // It's not possible to send invalid enum in a Multipart/form-data request
    // [Theory]
    // [ClassData(typeof(TestCultures))]
    // public async Task ErrorRecipeNotInEnum(string culture)
    // {
    //     var notInEnumDishType = ResourceErrorMessages.ResourceManager.GetString("RECIPE_DISH_TYPE_NOT_IN_ENUM", new CultureInfo(culture));
    //     var notInEnumDifficulty = ResourceErrorMessages.ResourceManager.GetString("RECIPE_DIFFICULTY_NOT_IN_ENUM", new CultureInfo(culture));
    //     var notInEnumCookingTime = ResourceErrorMessages.ResourceManager.GetString("RECIPE_COOKING_TIME_NOT_IN_ENUM", new CultureInfo(culture));
    //     List<string?> expectedErrorMessages = [notInEnumDishType, notInEnumDifficulty, notInEnumCookingTime ];
    //     var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
    //     var request = RequestRecipeFormBuilder.Build();
    //     
    //     var response = await inMemoryFactory.DoPostRecipeForm(
    //         "recipe/register",
    //         request,
    //         token: validToken,
    //         culture: culture);
    //     
    //     
    //     var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    //     
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     result.RootElement.GetProperty("errorMessages")
    //         .EnumerateArray()
    //         .Should()
    //         .Contain(e => expectedErrorMessages.Contains(e.GetString()));
    // }   
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorIngredientListEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_AT_LEAST_ONE_INGREDIENT", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        var request = RequestRecipeFormBuilder.Build();
        request.Ingredients = [];
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorIngredientEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INGREDIENT_NOT_EMPTY", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        var request = RequestRecipeFormBuilder.Build();
        request.Ingredients = ["   "];
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInstructionStepLessThanZero(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INSTRUCTION_STEP_GREATER_THAN_0", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        var request = RequestRecipeFormBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 0, Text = "Chop onions" }
        };
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInstructionTextEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INSTRUCTION_TEXT_NOT_EMPTY", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        var request = RequestRecipeFormBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = " " }
        };
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInstructionTextLessThan2000Characters(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INSTRUCTION_TEXT_LESS_THAN_2000", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        var request = RequestRecipeFormBuilder.Build();
        var overLimitText = new string('a', SharedValidators.MaximumRecipeInstructionTextLength + 1);
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = overLimitText }
        };
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInstructionStepMusBeUnique(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("RECIPE_INSTRUCTION_DUPLICATE_STEP_INSTRUCTION", new CultureInfo(culture));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inMemoryFactory.GetUser().Id);
        var request = RequestRecipeFormBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = "Step 1" },
            new() { Step = 1, Text = "Duplicate Step 1" }
        };
        
        var response = await inMemoryFactory.DoPostRecipeForm("recipe/register", request, token: validToken, culture: culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    
}