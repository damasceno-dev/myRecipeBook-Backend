using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.Entities;
using CommonTestUtilities.InLineData;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using Xunit;

namespace WebApi.Test.Users.RefreshTokens;

public class UserRefreshTokenControllerInMemoryTest(MyInMemoryFactory factory) : IClassFixture<MyInMemoryFactory>
{
    private const string Endpoint = "user/refresh-token";

    [Fact]
    public async Task Success()
    {
        var dbContext = factory.GetDbContext();
        var user = factory.GetUser();
        var refreshToken = factory.GetRefreshToken();
        var request = new RequestRefreshTokenJson { RefreshToken = refreshToken.Value };

        var response = await factory.DoPost(Endpoint, request);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.RootElement.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        result.RootElement.GetProperty("refreshToken").GetString().Should().NotBe(refreshToken.Value);

        var newRefreshToken = dbContext.RefreshTokens.SingleOrDefault(rt => rt.UserId == user.Id && rt.Value != refreshToken.Value);
        newRefreshToken.Should().NotBeNull();
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorInvalidRefreshToken(string culture)
    {
        var request = new RequestRefreshTokenJson { RefreshToken = "invalid-refresh-token" };
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("REFRESH_TOKEN_INVALID", new CultureInfo(culture));

        var response = await factory.DoPost(Endpoint, request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString() == expectedErrorMessage);
    }

    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task ErrorExpiredRefreshToken(string culture)
    {
        var dbContext = factory.GetDbContext();
        var user = factory.GetUser();
        var expiredRefreshToken = RefreshTokenBuilder.Build(user, expired: true);
        await dbContext.RefreshTokens.AddAsync(expiredRefreshToken);
        await dbContext.SaveChangesAsync();

        var request = new RequestRefreshTokenJson { RefreshToken = expiredRefreshToken.Value };
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("REFRESH_TOKEN_EXPIRED", new CultureInfo(culture));

        var response = await factory.DoPost(Endpoint, request, culture);
        var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.RootElement.GetProperty("errorMessages")
            .EnumerateArray()
            .Should()
            .ContainSingle(e => e.GetString() == expectedErrorMessage);
    }
}
