using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace WebApi.Test.Users.LoginExternal;

public class LoginGoogleUserControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    public const string GoogleEmail = "testuser@gmail.com";
    public const string GoogleName = "Test User";

    [Fact]
    public async Task GoogleLogin_RedirectsWithTokenOnSuccess()
    {
        const string url = "user/login/google?returnUrl=/redirect-url";

        var response = await factory.DoGet(url);
        
        if (response.RequestMessage?.RequestUri is null)
        {
            Assert.Fail("Failed to get the redirect url in Google login test");
        }
        var redirectLocation =  new Uri(response.RequestMessage.RequestUri.ToString());
        var query = System.Web.HttpUtility.ParseQueryString(redirectLocation.Query);
        query["token"].Should().NotBeNullOrEmpty(); // Token is present
        query["token"].Should().MatchRegex(@"^[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+$"); // JWT format
        query["name"].Should().Be(GoogleName);
        query["email"].Should().Be(GoogleEmail);
    }
    
    [Fact]
    public async Task LogoutSuccess()
    {
        var response = await factory.DoPost<object>("user/logout", default!);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseBody);

        result.RootElement.GetProperty("message").GetString().Should().Be("Logged out successfully");
    }

}