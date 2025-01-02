using System.Net;
using CommonTestUtilities.Token;
using FluentAssertions;
using Xunit;

namespace WebApi.Test.Users.Delete;

public class QueueDeleteUserControllerInMemoryTest(MyInMemoryFactory factory): IClassFixture<MyInMemoryFactory>
{
    [Fact]
    private async Task Success()
    {
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(factory.GetUser().Id);
        var response = await factory.DoDelete("user/delete", token:validToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}