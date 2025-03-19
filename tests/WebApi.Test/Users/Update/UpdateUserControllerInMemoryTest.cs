using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using Xunit;

namespace WebApi.Test.Users.Update;

public class UpdateUserControllerInMemoryTest : IClassFixture<MyInMemoryFactory>
{
    private readonly MyInMemoryFactory _factory;
    public UpdateUserControllerInMemoryTest(MyInMemoryFactory inMemoryFactory)
    {
        _factory = inMemoryFactory;
    }
    
    [Fact]
    private async Task SuccessFromResponseBodyInMemory()
    {
        var registeredUser = _factory.GetUser();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(registeredUser.Id);
        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        
        var response = await _factory.DoPut("user/update", request: requestUpdate, token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("name").GetString().Should().NotBe(registeredUser.Name);
        result.RootElement.GetProperty("email").GetString().Should().NotBe(registeredUser.Email);
        result.RootElement.GetProperty("name").GetString().Should().Be(requestUpdate.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(requestUpdate.Email);
    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeContainer()
    {
        var registeredUser = _factory.GetUser();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(registeredUser.Id);
        
        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        var response = await _factory.DoPut("user/update", request: requestUpdate, token: validToken);
        var profileFromJson = await response.Content.ReadFromJsonAsync<ResponseUserProfileJson>();
        
        if (profileFromJson is not null)
        {
            profileFromJson.Name.Should().NotBe(registeredUser.Name);
            profileFromJson.Email.Should().NotBe(registeredUser.Email);
            profileFromJson.Name.Should().Be(requestUpdate.Name);
            profileFromJson.Email.Should().Be(requestUpdate.Email);
        }
        else
        {
            Assert.Fail("Profile JSON deserialization resulted in null.");
        }
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailInvalid(string cultureFromRequest)
    {
        var registeredUser = _factory.GetUser();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(registeredUser.Id);
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo(cultureFromRequest));

        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        requestUpdate.Email = "invalid_email.com";
        var response = await _factory.DoPut("user/update", token: validToken, request:requestUpdate, culture:cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task EmailAlreadyExists(string cultureFromRequest)
    {
        var registeredUser = _factory.GetUser();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(registeredUser.Id);
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_ALREADY_EXISTS", new CultureInfo(cultureFromRequest));

        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        requestUpdate.Email = registeredUser.Email;
        var response = await _factory.DoPut("user/update", token: validToken, request:requestUpdate, culture:cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}