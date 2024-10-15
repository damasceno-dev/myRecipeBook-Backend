using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Infrastructure;
using Xunit;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

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
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorEmailEmpty(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_EMPTY", new CultureInfo
            (culture));
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = " ";
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        
        var response = await _httpClient.PostAsJsonAsync("user/login", request);
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
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        
        var response = await _httpClient.PostAsJsonAsync("user/login", request);
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
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_REGISTERED", new CultureInfo
            (culture));
        var request = RequestUserLoginJsonBuilder.Build();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        
        var response = await _httpClient.PostAsJsonAsync("user/login", request);
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
        var responseRegister = await _httpClient.PostAsJsonAsync("user/register", requestRegister);
        var userFromJson = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var userInDb = await _dbContextInMemory.Users.FindAsync(userFromJson!.Id);
        userInDb!.Active = false;
        await _dbContextInMemory.SaveChangesAsync();
        
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        var response = await _httpClient.PostAsJsonAsync("user/login", new RequestUserLoginJson {Email = 
            requestRegister.Email, Password = requestRegister.Password});
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
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("WRONG_PASSWORD", new CultureInfo(culture));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        await _httpClient.PostAsJsonAsync("user/register", requestRegister);
        
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        var response = await _httpClient.PostAsJsonAsync("user/login", new RequestUserLoginJson {Email = 
            requestRegister.Email, Password = "wrong.password123"});
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }    
}