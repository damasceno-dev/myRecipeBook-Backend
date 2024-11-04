using Microsoft.AspNetCore.Http;
using Moq;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace CommonTestUtilities.Token;

public class HttpContextAccessorBuilder
{
    private readonly Mock<IHttpContextAccessor> _contextAccessor;
    public HttpContextAccessorBuilder()
    {
        _contextAccessor = new Mock<IHttpContextAccessor>();
    }

    public HttpContextAccessorBuilder WithNullHttpContextAccessor()
    {
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns((HttpContext)null!);
        return this;
    }
    public IHttpContextAccessor Build()
    {
        return _contextAccessor.Object;
    }
}