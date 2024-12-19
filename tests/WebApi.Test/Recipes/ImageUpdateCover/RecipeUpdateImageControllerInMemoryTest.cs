using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.FormFile;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Communication;
using Xunit;

namespace WebApi.Test.Recipes.ImageUpdateCover;

public class RecipeUpdateImageControllerInMemoryTest(MyInMemoryFactory factory): IClassFixture<MyInMemoryFactory>
{
    [Theory]
    [InlineData("jpgImage.jpg")]
    [InlineData("pngImage.png")]
    public async Task Success(string imageName)
    {
        var recipeToUpdateId = factory.GetRecipes().First().Id;
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        var file = FormFileBuilder.Build(imageName);
        var content = new MultipartFormDataContent
        {
            { new StreamContent(file.OpenReadStream()), "file", imageName }
        };
        
        var response = await factory.DoPutMultipartForm($"update/image/{recipeToUpdateId}", content, token: validToken);
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var recipeInDb = await factory.GetDbContext().Recipes.FindAsync(recipeToUpdateId);
        if (recipeInDb is null)
        {
            Assert.Fail("recipeInDb is null for image update test.");
        }
        else
        {
            await factory.GetDbContext().Entry(recipeInDb).ReloadAsync();
            recipeInDb.ImageIdentifier.Should().NotBeNullOrEmpty();
            recipeInDb.ImageIdentifier.Should().EndWith(Path.GetExtension(imageName));
        }
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInvalidFileType(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("IMAGE_INVALID_TYPE", new CultureInfo(culture));
        var recipeToUpdateId = factory.GetRecipes().First().Id;
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        var file = FormFileBuilder.Build("textFile.txt");
        var content = new MultipartFormDataContent
        {
            { new StreamContent(file.OpenReadStream()), "file", "textFile.txt" }
        };
        
        var response = await factory.DoPutMultipartForm($"update/image/{recipeToUpdateId}", content, token: validToken, culture:culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }  

}