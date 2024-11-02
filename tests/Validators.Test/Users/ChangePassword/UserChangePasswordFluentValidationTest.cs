using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.ChangePassword;
using MyRecipeBook.Communication;
using Xunit;

namespace Validators.Test.Users.ChangePassword;

public class UserChangePasswordFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(6);
        var result = new UserChangePasswordFluentValidation().Validate(request);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Count.Should().Be(0);
    }
    
    [Fact]
    public void ErrorNewPasswordEmpty()
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(6);
        request.NewPassword = string.Empty;
        var result = new UserChangePasswordFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.PASSWORD_EMPTY);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ErrorNewPasswordLength(int passwordLength)
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(passwordLength);
        var result = new UserChangePasswordFluentValidation().Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.PASSWORD_LENGTH);
    }
}