using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Communication.Requests;
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
}