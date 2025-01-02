using System.Net;
using FluentAssertions;
using Xunit;

namespace WebApi.Test.Users.ResetPassword;

public class UserGetResetPasswordCodeControllerInMemoryTest(MyInMemoryFactory factory): IClassFixture<MyInMemoryFactory>
{
    [Fact]
    private async Task SuccessFromResponseBodyInMemory()
    {
        var email = factory.GetUser().Email;
        var response = await factory.DoGet($"user/get-reset-password-code/{email}");
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
}