using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Tokens;
using Xunit;

namespace WebApi.Test.Token;

public class TokenTests
{
    [Fact]
    public void ErrorNullContext()
    {
        var httpContextAccessor = new HttpContextAccessorBuilder().WithNullHttpContextAccessor().Build();

        var tokenProvider = new GetTokenValueFromRequest(httpContextAccessor);
        Action act = () => tokenProvider.Value();
        
        act.Should().Throw<ArgumentException>().WithMessage("Contexto nulo");

    }
}