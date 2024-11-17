using System.Globalization;
using System.Net;
using System.Text.Json;
using CommonTestUtilities.InLineData;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Communication;
using MyRecipeBook.Tokens;
using Xunit;
using Xunit.Abstractions;

namespace WebApi.Test.Token;

public class TokenTests : IClassFixture<MyInMemoryFactory>
{
    private readonly MyInMemoryFactory _factory;
    private readonly TokenTestHelper _helper;

    public TokenTests(MyInMemoryFactory inMemoryFactory, ITestOutputHelper output)
    {
        _factory = inMemoryFactory;
        _helper = new TokenTestHelper(output);
    }
    
    [Fact]
    public void ErrorNullContext()
    {
        var httpContextAccessor = new HttpContextAccessorBuilder().WithNullHttpContextAccessor().Build();

        var tokenProvider = new GetTokenValueFromRequest(httpContextAccessor);
        Action act = () => tokenProvider.Value();
        
        act.Should().Throw<ArgumentException>().WithMessage("Contexto nulo");
    }
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task TokenWithNoPermission(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("TOKEN_WITH_NO_PERMISSION", new CultureInfo(cultureFromRequest));
        var validToken = JsonWebTokenRepositoryBuilder.Build().Generate(new Guid());

        var responses = await _helper.ExecuteAllRoutes(_factory, cultureFromRequest, validToken);
        foreach (var (route, response) in responses)
        {    
            var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"Route: {route}");
            result.RootElement.GetProperty("errorMessages")
                .EnumerateArray()
                .Should()
                .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
        }
    } 
    
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task TokenEmpty(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("TOKEN_EMPTY", new CultureInfo(cultureFromRequest));
        var emptyToken = string.Empty;

        var responses = await _helper.ExecuteAllRoutes(_factory, cultureFromRequest, emptyToken);
        foreach (var (route, response) in responses)
        {    
            var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"Route: {route}");
            result.RootElement.GetProperty("errorMessages")
                .EnumerateArray()
                .Should()
                .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
        }
    }   
    [Theory]
    [ClassData(typeof(TestCultures))]
    public async Task TokenExpired(string cultureFromRequest)
    {
        var expectedErrorMessage = ResourceErrorMessages.ResourceManager.GetString("TOKEN_EXPIRED", new CultureInfo(cultureFromRequest));
        var expiredToken = JsonWebTokenRepositoryBuilder.BuildExpiredToken().Generate(_factory.GetUser().Id);
        // Wait to ensure the token is expired
        await Task.Delay(TimeSpan.FromSeconds(0.1));
        
        var responses = await _helper.ExecuteAllRoutes(_factory, cultureFromRequest, expiredToken);
        foreach (var (route, response) in responses)
        {    
            var result = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"Route: {route}");
            result.RootElement.GetProperty("errorMessages")
                .EnumerateArray()
                .Should()
                .ContainSingle(e => e.GetString()!.Equals(expectedErrorMessage));
        }
    }
}