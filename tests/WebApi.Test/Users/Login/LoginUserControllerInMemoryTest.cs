using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Infrastructure;
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
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        await _factory.DoPost("user/register", requestRegister);
        var request = new RequestUserLoginJson { Email = requestRegister.Email, Password = requestRegister.Password };
        
        var response = await _factory.DoPost("user/login", request);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("name").GetString().Should().Be(requestRegister.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(requestRegister.Email);
        result.RootElement.GetProperty("responseToken").GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeInMemory()
    {
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var request = new RequestUserLoginJson { Email = requestRegister.Email, Password = requestRegister.Password };
        await _factory.DoPost("user/register", requestRegister);
        
        var response = await _factory.DoPost("user/login", request);
        var userFromJson = await response.Content.ReadFromJsonAsync<ResponseUserLoginJson>();
        var userInDb = await _factory.GetDbContext().Users.FirstAsync(u => u.Email.Equals(userFromJson!.Email) && u.Name
            .Equals(userFromJson.Name));
        
        userInDb.Should().NotBeNull();
        userInDb!.Name.Should().Be(requestRegister.Name);
        userInDb!.Email.Should().Be(requestRegister.Email);
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_EMPTY", new CultureInfo(culture));
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = " ";
        
        var response = await _factory.DoPost("user/register", request, culture);
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
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo
            (culture));
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
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var request = new RequestUserLoginJson { Email = requestRegister.Email, Password = requestRegister.Password };
        var userFromJson = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var userInDb = await _factory.GetDbContext().Users.SingleAsync(u => u.Email == userFromJson!.Email);
        userInDb!.Active = false;
        await _factory.GetDbContext().SaveChangesAsync();
        
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
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        await _factory.DoPost("user/register", requestRegister);
        var request = new RequestUserLoginJson {Email = requestRegister.Email,Password = "wrong.password123"};
        
        var response = await _factory.DoPost("user/login", request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
}