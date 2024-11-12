using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Cryptography;
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

namespace WebApi.Test.Users.ChangePassword;

public class ChangePasswordUserControllerInMemoryTest: IClassFixture<MyInMemoryFactory>
{
    private readonly MyInMemoryFactory _factory;

    public ChangePasswordUserControllerInMemoryTest(MyInMemoryFactory inMemoryFactory)
    {
        _factory = inMemoryFactory;
    }

    [Fact]
    private async Task Success()
    {
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(7);
        requestChangePassword.CurrentPassword = requestRegister.Password;

        var response = await _factory.DoPut("user/changePassword", request: requestChangePassword, token: validToken);
        var userInDb = await _factory.RecipeDbContext.Users.SingleAsync(u => u.Email == requestRegister.Email);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        PasswordEncryptionBuilder.Build().VerifyPassword(requestRegister.Password, userInDb.Password).Should().Be(false);
        PasswordEncryptionBuilder.Build().VerifyPassword(requestChangePassword.NewPassword, userInDb.Password).Should().Be(true);
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorNewPasswordEmpty(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("PASSWORD_EMPTY", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(7);
        requestChangePassword.NewPassword = string.Empty;

        var response = await _factory.DoPut("user/changePassword", request: requestChangePassword, token: validToken, culture: cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestPasswordLengthsAndCultures))]
    public async Task ErrorPasswordLength(int passwordLength,string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("PASSWORD_LENGTH", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(passwordLength);

        var response = await _factory.DoPut("user/changePassword", request: requestChangePassword, token: validToken, culture: cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorPasswordWrong(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("PASSWORD_WRONG", new CultureInfo(cultureFromRequest));
        var requestRegister = RequestUserRegisterJsonBuilder.Build();
        var responseRegister = await _factory.DoPost("user/register", requestRegister);
        var resultRegister = await responseRegister.Content.ReadFromJsonAsync<ResponseUserRegisterJson>();
        var validToken = resultRegister!.ResponseToken.Token;
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(7);
        requestChangePassword.CurrentPassword = "wrong_password";

        var response = await _factory.DoPut("user/changePassword", request: requestChangePassword, token: validToken, culture: cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

        var response = await _factory.DoPut("user/changePassword",request: RequestUserUpdateJsonBuilder.Build(), token: validToken, culture:cultureFromRequest);
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

        var response = await _factory.DoPut("user/changePassword",request: RequestUserUpdateJsonBuilder.Build(), token: emptyToken, culture:cultureFromRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

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
        var user = await _factory.RecipeDbContext.Users.FirstOrDefaultAsync(u => u.Email == requestRegister.Email && u.Name == requestRegister.Name);
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

        var response = await _factory.DoPut("user/changePassword",request: RequestUserUpdateJsonBuilder.Build(), token: expiredToken, culture:cultureFromRequest);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    #endregion
    
}