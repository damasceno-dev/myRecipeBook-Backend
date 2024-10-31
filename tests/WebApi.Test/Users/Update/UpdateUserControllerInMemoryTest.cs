using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace WebApi.Test.Users.Update;

public class UpdateUserControllerInMemoryTest : IClassFixture<MyInMemoryFactory>
{
    private readonly MyRecipeBookDbContext _dbContextInMemory;
    private readonly MyInMemoryFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public UpdateUserControllerInMemoryTest(MyInMemoryFactory inMemoryFactory, ITestOutputHelper testOutputHelper)
    {
        _factory = inMemoryFactory;
        _testOutputHelper = testOutputHelper;
        _dbContextInMemory = inMemoryFactory.Services.GetRequiredService<MyRecipeBookDbContext>();
    }
    
    [Fact]
    private async Task SuccessFromResponseBodyInMemory()
    {
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await JsonDocument.ParseAsync(await responseRegister.Content.ReadAsStreamAsync());
        var validToken = resultRegister.RootElement.GetProperty("responseToken").GetProperty("token").GetString();

        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        var response = await _factory.DoPut("user/update", request: requestUpdate, token: validToken);
        var responseBody = await response.Content.ReadAsStreamAsync();
        var result = await JsonDocument.ParseAsync(responseBody);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("name").GetString().Should().NotBe(requestRegister.Name);
        result.RootElement.GetProperty("email").GetString().Should().NotBe(requestRegister.Email);
        result.RootElement.GetProperty("name").GetString().Should().Be(requestUpdate.Name);
        result.RootElement.GetProperty("email").GetString().Should().Be(requestUpdate.Email);
    }
    
    [Fact]
    private async Task SuccessFromJsonSerializeContainer()
    {
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;
        
        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        var response = await _factory.DoPut("user/update", request: requestUpdate, token: validToken);
        var profileFromJson = await response.Content.ReadFromJsonAsync<ResponseUserProfileJson>();
        
        if (profileFromJson is not null)
        {
            profileFromJson.Name.Should().NotBe(requestRegister.Name);
            profileFromJson.Email.Should().NotBe(requestRegister.Email);
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
    public async Task ErrorEmailEmpty(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_EMPTY", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;

        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        requestUpdate.Email = string.Empty;
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
    public async Task ErrorEmailInvalid(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;

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
    public async Task ErrorNameEmpty(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("NAME_NOT_EMPTY", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;

        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        requestUpdate.Name = string.Empty;
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
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_ALREADY_EXISTS", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;

        var requestUpdate = RequestUserUpdateJsonBuilder.Build();
        requestUpdate.Email = requestRegister.Email;
        var response = await _factory.DoPut("user/update", token: validToken, request:requestUpdate, culture:cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    #region TokenTests
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task TokenWithNoPermission(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("TOKEN_WITH_NO_PERMISSION", new CultureInfo(cultureFromRequest));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(new Guid());

        var response = await _factory.DoPut("user/update",request: RequestUserUpdateJsonBuilder.Build(), token: validToken, culture:cultureFromRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    } 
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task TokenEmpty(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("TOKEN_EMPTY", new CultureInfo(cultureFromRequest));
        var emptyToken = string.Empty;

        var response = await _factory.DoPut("user/update",request: RequestUserUpdateJsonBuilder.Build(), token: emptyToken, culture:cultureFromRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        _testOutputHelper.WriteLine(result.RootElement.GetProperty("errorMessages").ToString());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }   
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task TokenExpired(string cultureFromRequest)
    {
        var expiredToken = string.Empty;
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("TOKEN_EXPIRED", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        await _factory.DoPost("user/register", requestRegister);
        var user = await _dbContextInMemory.Users.FirstOrDefaultAsync(u => u.Email == requestRegister.Email && u.Name == requestRegister.Name);
        if (user is not null)
        {
            expiredToken = JsonWebTokenRepositoryBuilder.BuildExpiredToken().Generate(user.Id);
        }
        else
        {
            Assert.Fail("User from test register is null");
        }
        // Wait to ensure the token is expired
        await Task.Delay(TimeSpan.FromSeconds(0.1));

        var response = await _factory.DoPut("user/update",request: RequestUserUpdateJsonBuilder.Build(), token: expiredToken, culture:cultureFromRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }

    #endregion
   
}