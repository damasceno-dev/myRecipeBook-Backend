using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.ResetPassword;
using MyRecipeBook.Communication;
using Xunit;

namespace Validators.Test.Users.ResetPassword;

public class UserResetPasswordFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestUserResetPasswordJsonBuilder.Build();
        var result = new UserResetPasswordFluentValidation().Validate(request);
        
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ErrorPasswordInvalid(int passwordLength)
    {
        var request = RequestUserResetPasswordJsonBuilder.Build(passwordLength);
        var result = new UserResetPasswordFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.PASSWORD_LENGTH);
    }
}