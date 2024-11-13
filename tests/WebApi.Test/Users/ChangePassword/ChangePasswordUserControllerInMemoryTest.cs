using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.Cryptography;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
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
        var registeredUser = _factory.GetUser();
        var currentPassword = _factory.GetPassword();
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(registeredUser.Id);
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(SharedValidators.MinimumPasswordLength + 1);
        requestChangePassword.CurrentPassword = currentPassword;

        var response = await _factory.DoPut("user/changePassword", request: requestChangePassword, token: validToken);
        await _factory.GetDbContext().Entry(registeredUser).ReloadAsync();
        var userInDb = await _factory.GetDbContext().Users.SingleAsync(u => u.Email == registeredUser.Email);
        var responseLoginNew = await _factory.DoPost("user/login", new RequestUserLoginJson { Email = registeredUser.Email, Password = requestChangePassword.NewPassword });
        var responseLoginOld = await _factory.DoPost("user/login", new RequestUserLoginJson { Email = registeredUser.Email, Password = currentPassword });
        
        responseLoginNew.StatusCode.Should().Be(HttpStatusCode.OK);
        responseLoginOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        PasswordEncryptionBuilder.Build().VerifyPassword(currentPassword, userInDb.Password).Should().Be(false);
        PasswordEncryptionBuilder.Build().VerifyPassword(requestChangePassword.NewPassword, userInDb.Password).Should().Be(true);
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorNewPasswordEmpty(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("PASSWORD_EMPTY", new CultureInfo(cultureFromRequest));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(_factory.GetUser().Id);
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(SharedValidators.MinimumPasswordLength + 1);
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
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(_factory.GetUser().Id);
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
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(_factory.GetUser().Id);
        var requestChangePassword = RequestUserChangePasswordJsonBuilder.Build(SharedValidators.MinimumPasswordLength + 1);
        requestChangePassword.CurrentPassword = "wrong_password";

        var response = await _factory.DoPut("user/changePassword", request: requestChangePassword, token: validToken, culture: cultureFromRequest);
        
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}