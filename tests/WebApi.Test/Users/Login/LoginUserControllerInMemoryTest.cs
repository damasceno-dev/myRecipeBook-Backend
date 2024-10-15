using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Infrastructure;
using Xunit;

namespace WebApi.Test.Users.Login;

public class LoginUserControllerInMemoryTest : IClassFixture<MyInMemoryFactory>
{
    private readonly MyRecipeBookDbContext _dbContextInMemory;
    private readonly HttpClient _httpClient;

    public LoginUserControllerInMemoryTest(MyInMemoryFactory inMemoryFactory)
    {
        _httpClient = inMemoryFactory.CreateClient();
        _dbContextInMemory = inMemoryFactory.Services.GetRequiredService<MyRecipeBookDbContext>();
    }
    
    [Fact]
    private async Task SuccessFromResponseBodyInMemory()
    {
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        await _httpClient.PostAsJsonAsync("user/register", requestRegister);
        
        var response = await _httpClient.PostAsJsonAsync("user/login", new RequestUserLoginJson {Email = 
            requestRegister.Email, Password = requestRegister.Password});
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("name").GetString().Should().Be(requestRegister.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(requestRegister.Email);
        

    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeInMemory()
    {
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        await _httpClient.PostAsJsonAsync("user/register", requestRegister);
        
        var response = await _httpClient.PostAsJsonAsync("user/login", new RequestUserLoginJson {Email = 
            requestRegister.Email, Password = requestRegister.Password});
        var userFromJson = await response.Content.ReadFromJsonAsync<ResponseUserLoginJson>();
        var userInDb = await _dbContextInMemory.Users.FirstAsync(u => u.Email.Equals(userFromJson!.Email) && u.Name
            .Equals(userFromJson.Name));
        
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be(requestRegister.Name);
        userInDb!.Email.Should().Be(requestRegister.Email);
    }
}