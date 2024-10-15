using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using Xunit;

namespace Validators.Test.Users.Login;

public class UserLoginFluentValidationTest
{
    [Fact]
    public void Success()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        var result = new UserLoginFluentValidation().Validate(request);

        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Count.Should().Be(0);
    }

    [Fact]
    public void ErrorEmailEmpty()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = " ";
        var result = new UserLoginFluentValidation().Validate(request);

        result.Should().NotBeNull();
        result.Errors.Select(e => e.ErrorMessage).Should().ContainSingle(m => m.Equals(ResourceErrorMessages
            .EMAIL_NOT_EMPTY));
    }

    [Fact]
    public void ErrorEmailInvalid()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = "invalidEmail";
        var result = new UserLoginFluentValidation().Validate(request);

        result.Should().NotBeNull();
        result.Errors.Select(e => e.ErrorMessage)
            .Should()
            .ContainSingle(m => m.Equals(ResourceErrorMessages.EMAIL_INVALID));
    }
}