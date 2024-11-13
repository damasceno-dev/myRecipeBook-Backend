using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Entities;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using Xunit;

namespace WebApi.Test.Users.Profile;

public class GetProfileWithTokenControllerInMemoryTest : IClassFixture<MyInMemoryFactory>
{
    private readonly MyInMemoryFactory _factory;
    public GetProfileWithTokenControllerInMemoryTest(MyInMemoryFactory inMemoryFactory)
    {
        _factory = inMemoryFactory;
    }
    
    [Fact]
    private async Task SuccessFromResponseBodyInMemory()
    {
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(_factory.GetUser().Id);

        var response = await _factory.DoGet("user/getProfileWithToken", token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("name").GetString().Should().Be(_factory.GetUser().Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(_factory.GetUser().Email);
    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeContainer()
    {
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(_factory.GetUser().Id);
        
        var response = await _factory.DoGet("user/getProfileWithToken", token: validToken);
        var profileFromJson = await response.Content.ReadFromJsonAsync<ResponseUserProfileJson>();
        
        if (profileFromJson is not null)
        {
            profileFromJson.Name.Should().Be(_factory.GetUser().Name);
            profileFromJson.Email.Should().Be(_factory.GetUser().Email);
        }
        else
        {
            Assert.Fail("Profile JSON deserialization resulted in null.");
        }
    }
        
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task InactiveUser(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_ACTIVE", new CultureInfo(cultureFromRequest));
        var (inactiveUser, _) = UserBuilder.Build();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(inactiveUser.Id);
        inactiveUser.Active = false;
        _factory.GetDbContext().Add(inactiveUser);
        await _factory.GetDbContext().SaveChangesAsync();

        var response = await _factory.DoGet("user/getProfileWithToken", token: validToken, culture:cultureFromRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}