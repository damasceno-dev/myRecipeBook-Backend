using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Infrastructure;
using Xunit;

namespace WebApi.Test.Users.Register;

public class RegisterUserControllerTest : IClassFixture<MyFactory>
{
    private readonly MyFactory _factory;
    private readonly MyRecipeBookDbContext _dbContext;

    public RegisterUserControllerTest(MyFactory factory)
    {
        _factory = factory;
        _dbContext = factory.Services.GetRequiredService<MyRecipeBookDbContext>();
    }
    
    [Fact]
    public async Task SuccessFromResponseBody()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("user/register", request);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.RootElement.GetProperty("name").GetString().Should().Be(request.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(request.Email);
    }
    
    [Fact]
    public async Task SuccessFromJsonSerialize()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("user/register", request);
        var userFromJson = await response.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var userInDb = await _dbContext.Users.FindAsync(userFromJson!.Id);
        
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be(request.Name);
        userInDb!.Email.Should().Be(request.Email);
    }
}