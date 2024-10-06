using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Communication;
using Xunit;

namespace Validators.Test.Users.Register;

public class UserRegisterFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var result = new UserRegisterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeTrue();
    }
    [Fact]
    public void ErrorNameEmpty()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        request.Name = string.Empty;
        var result = new UserRegisterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.NAME_NOT_EMPTY);
    }
    [Fact]
    public void ErrorEmailEmpty()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        request.Email = string.Empty;
        var result = new UserRegisterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.EMAIL_NOT_EMPTY);
    }
    [Fact]
    public void ErrorEmailInvalid()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        request.Email = "invalid.email";
        var result = new UserRegisterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.EMAIL_INVALID);
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ErrorPasswordInvalid( int passwordLength)
    {
        var request = RequestUserRegisterJsonBuilder.Build(passwordLength);
        var result = new UserRegisterFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.PASSWORD_LENGTH);
    }
}