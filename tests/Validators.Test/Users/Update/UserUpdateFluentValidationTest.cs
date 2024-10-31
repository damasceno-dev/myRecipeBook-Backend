using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Application.UseCases.Users.Update;
using MyRecipeBook.Communication;
using Xunit;

namespace Validators.Test.Users.Update;

public class UserUpdateFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        var result = new UserUpdateFluentValidation().Validate(request);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Count.Should().Be(0);
    }
    
    [Fact]
    public void ErrorNameEmpty()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        request.Name = string.Empty;
        var result = new UserUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.NAME_NOT_EMPTY);
    }
    [Fact]
    public void ErrorEmailEmpty()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        request.Email = string.Empty;
        var result = new UserUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.EMAIL_NOT_EMPTY);
    }
    [Fact]
    public void ErrorEmailInvalid()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        request.Email = "invalid.email";
        var result = new UserUpdateFluentValidation().Validate(request);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Select(v => v.ErrorMessage)
            .Should().ContainSingle(ResourceErrorMessages.EMAIL_INVALID);
    }
}