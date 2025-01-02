using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.Cryptography;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Users.ResetPassword;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using Xunit;

namespace WebApi.Test.Users.ResetPassword;

public class UserResetPasswordControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    [Fact]
    private async Task Success()
    {
        var newPassword = RequestUserResetPasswordJsonBuilder.Build().NewPassword;
        var user = factory.GetUser();
        var userCode = factory.GetResetPasswordCode();
        var request = new RequestUserResetPasswordJson() { Code = userCode.Code, Email = user.Email, NewPassword = newPassword };
        
        var response = await factory.DoPost("user/reset-password", request);        
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await factory.GetDbContext().Entry(user).ReloadAsync();
        
        var updatedUser = factory.GetDbContext().Users.First(u => u.Id == user.Id);
        PasswordEncryptionBuilder.Build().VerifyPassword(newPassword, updatedUser.Password).Should().Be(true);

        var deactivatedCodes = factory.GetDbContext().UserPasswordResetCodes
            .Where(code => code.UserId == user.Id && !code.Active)
            .ToList();
        deactivatedCodes.Should().NotBeEmpty();
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorUserNull(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("EMAIL_NOT_REGISTERED", new CultureInfo(culture));
        var request = RequestUserResetPasswordJsonBuilder.Build();
        
        var response = await factory.DoPost("user/reset-password", request, culture); 
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorUserPasswordCodeNull(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("USER_PASSWORD_RESET_INVALID_CODE", new CultureInfo(culture));
        var newPassword = RequestUserResetPasswordJsonBuilder.Build().NewPassword;
        var user = factory.GetUser();
        var request = new RequestUserResetPasswordJson() { Email = user.Email, NewPassword = newPassword };
        
        var response = await factory.DoPost("user/reset-password", request, culture); 
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorWrongCode(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("USER_PASSWORD_RESET_WRONG_CODE", new CultureInfo(culture));
        var newPassword = RequestUserResetPasswordJsonBuilder.Build().NewPassword;
        var user = factory.GetUser();
        var wrongCode = DigitGenerator.Generate6DigitCode();        
        while (wrongCode == factory.GetResetPasswordCode().Code)
        {
            wrongCode = DigitGenerator.Generate6DigitCode();
        }
        var request = new RequestUserResetPasswordJson() { Code = wrongCode, Email = user.Email, NewPassword = newPassword };

        
        var response = await factory.DoPost("user/reset-password", request, culture); 
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorExpiredCode(string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("USER_PASSWORD_RESET_WRONG_CODE", new CultureInfo(culture));
        var newPassword = RequestUserResetPasswordJsonBuilder.Build().NewPassword;
        var user = factory.GetUser();
        const int expiredTime = UserResetPasswordUseCase.CodeExpirationTimeInMinutes + 1;
        var expiredCode = DigitGenerator.Generate6DigitCode();
        var userPasswordResetCode = new UserPasswordResetCode
        {
            UserId = user.Id,
            Code = expiredCode,
            CreatedOn = DateTime.UtcNow.AddMinutes(-expiredTime)
        };
        await factory.GetDbContext().UserPasswordResetCodes.AddAsync(userPasswordResetCode);
        await factory.GetDbContext().SaveChangesAsync();
        var request = new RequestUserResetPasswordJson() { Code = expiredCode, Email = user.Email, NewPassword = newPassword };
        
        var response = await factory.DoPost("user/reset-password", request, culture); 
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
    
    [Theory]
    [ClassData(typeof(TestPasswordLengthsAndCultures))]
    public async Task ErrorPasswordInvalid(int passwordLength, string culture)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("PASSWORD_LENGTH", new CultureInfo(culture));
        var newPassword = RequestUserResetPasswordJsonBuilder.Build(passwordLength).NewPassword;
        var user = factory.GetUser();
        var userCode = factory.GetResetPasswordCode();
        var request = new RequestUserResetPasswordJson() { Code = userCode.Code, Email = user.Email, NewPassword = newPassword };

        var response = await factory.DoPost("user/reset-password", request, culture); 
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
    }
}