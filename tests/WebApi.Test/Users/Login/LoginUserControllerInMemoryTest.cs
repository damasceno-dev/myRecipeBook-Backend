using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Entities;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using Xunit;

namespace WebApi.Test.Users.Login;

public class LoginUserControllerInMemoryTest : IClassFixture<MyInMemoryFactory>
{
    private readonly MyInMemoryFactory _factory;

    public LoginUserControllerInMemoryTest(MyInMemoryFactory inMemoryFactory)
    {
        _factory = inMemoryFactory;
    }
    
    [Fact]
    private async Task SuccessFromResponseBodyInMemory()
    {
        var request = new RequestUserLoginJson { Email = _factory.GetUser().Email, Password = _factory.GetPassword() };
        
        var response = await _factory.DoPost("user/login", request);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("name").GetString().Should().Be(_factory.GetUser().Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(_factory.GetUser().Email);
        result.RootElement.GetProperty("responseToken").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeInMemory()
    {
        var request = new RequestUserLoginJson { Email = _factory.GetUser().Email, Password = _factory.GetPassword() };
        
        var response = await _factory.DoPost("user/login", request);
        var userFromJson = await response.Content.ReadFromJsonAsync<ResponseUserLoginJson>();
        var userInDb = await _factory.GetDbContext().Users.FirstAsync(u => u.Email.Equals(userFromJson!.Email) && u.Name.Equals(userFromJson.Name));
        
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be(_factory.GetUser().Name);
        userInDb!.Email.Should().Be(_factory.GetUser().Email);
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_EMPTY", new CultureInfo(culture));
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = " ";
        
        var response = await _factory.DoPost("user/login", request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailInvalid(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo(culture));
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = "invalid.email.com";
        
        var response = await _factory.DoPost("user/login", request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailNotRegistered(string culture)    
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_REGISTERED", new CultureInfo(culture));
        var request = RequestUserLoginJsonBuilder.Build();
        
        var response = await _factory.DoPost("user/login", request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailNotActive(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_ACTIVE", new CultureInfo(culture));
        var (inactiveUser, inactiveUserPassword) = UserBuilder.Build();
        inactiveUser.Active = false;
        _factory.GetDbContext().Add(inactiveUser);
        await _factory.GetDbContext().SaveChangesAsync();
        var request = new RequestUserLoginJson { Email = inactiveUser.Email, Password = inactiveUserPassword };
        
        var response = await _factory.DoPost("user/login", request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }   
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorWrongPassword(string culture)    
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("PASSWORD_WRONG", new CultureInfo(culture));
        var request = new RequestUserLoginJson {Email = _factory.GetUser().Email,Password = "wrong.password123"};
        
        var response = await _factory.DoPost("user/login", request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
}