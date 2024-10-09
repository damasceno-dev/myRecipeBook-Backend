using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Resources;
using System.Text.Json;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Infrastructure;
using Xunit;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace WebApi.Test.Users.Register;

public class RegisterUserControllerContainerTest : IClassFixture<MyContainerFactory>
{
    private readonly MyRecipeBookDbContext _dbContextContainer;
    private readonly MyContainerFactory _containerFactory;
    private readonly HttpClient _httpClient;

    public RegisterUserControllerContainerTest(MyContainerFactory containerFactory)
    {
        _containerFactory = containerFactory;
        _httpClient = containerFactory.CreateClient();
        _dbContextContainer = containerFactory.Services.GetRequiredService<MyRecipeBookDbContext>();
    }
    
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var response = await _httpClient.PostAsJsonAsync("user/register", request);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.RootElement.GetProperty("name").GetString().Should().Be(request.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(request.Email);
    }
    
    [Fact]
    public async Task SuccessFromJsonSerializeContainer()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        
        var response = await _httpClient.PostAsJsonAsync("user/register", request);
        var userFromJson = await response.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var userInDb = await _dbContextContainer.Users.FindAsync(userFromJson!.Id);
        
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be(request.Name);
        userInDb!.Email.Should().Be(request.Email);
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en")]
    public async Task ErrorNameEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("NAME_NOT_EMPTY", new CultureInfo
            (culture));
        var request = RequestUserRegisterJsonBuilder.Build();
        request.Name = "";
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        
        var response = await _httpClient.PostAsJsonAsync("user/register", request);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}