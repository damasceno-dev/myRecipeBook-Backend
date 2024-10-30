using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Infrastructure;
using Xunit;

namespace WebApi.Test.Users.Register;

public class RegisterUserControllerContainerTest : IClassFixture<MyContainerFactory>
{
    private readonly MyRecipeBookDbContext _dbContextContainer;
    private readonly MyContainerFactory _factory;

    public RegisterUserControllerContainerTest(MyContainerFactory containerFactory)
    {
        _factory = containerFactory;
        _dbContextContainer = containerFactory.Services.GetRequiredService<MyRecipeBookDbContext>();
    }
    
    [Fact]
    public async Task SuccessFromResponseBodyContainer()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var response = await _factory.DoPost("user/register", request);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.RootElement.GetProperty("name").GetString().Should().Be(request.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(request.Email);
        result.RootElement.GetProperty("responseToken").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task SuccessFromJsonSerializeContainer()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        
        var response = await _factory.DoPost("user/register", request);
        var userFromJson = await response.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var userInDb = await _dbContextContainer.Users.SingleAsync(u => u.Email == userFromJson!.Email);
        
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be(request.Name);
        userInDb.Email.Should().Be(request.Email);
    }

}