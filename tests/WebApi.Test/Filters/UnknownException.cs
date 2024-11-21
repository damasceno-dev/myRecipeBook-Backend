using CommonTestUtilities.InLineData;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using Xunit;

namespace WebApi.Test.Filters;

public class UnknownException
{
    [Theory]
    [ClassData(typeof(TestEnvironments))]
    public void HandlesUnknownExceptionCorrectly(string environment)
    {
        var expectedMessage = environment == "Development"
            ? "Test exception occurred"
            : ResourceErrorMessages.ResourceManager.GetString("UNKNOWN_ERROR");

        var exceptionFilter = new MyRecipeBook.Filters.ExceptionFilter();

        var environmentMock = new EnvironmentBuilder().SetEnvironmentName(environment).Build();
        
        var context = new DefaultHttpContext
        {
            Request = {Method = "GET",Path = "/test"},
            RequestServices = new ServiceCollection()
                .AddSingleton<IWebHostEnvironment>(environmentMock)
                .BuildServiceProvider()
        };

        var exceptionContext = new ExceptionContext(
            new ActionContext(context, new RouteData(), new ControllerActionDescriptor()),
            new List<IFilterMetadata>()
        )
        {
            Exception = new Exception("Test exception occurred")
        };

        exceptionFilter.OnException(exceptionContext);
        
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        ((exceptionContext.Result as ObjectResult)?.Value as ResponseErrorJson)!.ErrorMessages.Should().ContainSingle(e => e == expectedMessage);
    }
}

public class EnvironmentBuilder
{
    private readonly Mock<IWebHostEnvironment> _repository;


    public EnvironmentBuilder()
    {
        _repository = new Mock<IWebHostEnvironment>();
    }
    public EnvironmentBuilder SetEnvironmentName(string environmentName)
    {
        _repository.Setup(e => e.EnvironmentName).Returns(environmentName);
        
        return this;
    }

    public IWebHostEnvironment Build()
    {
        return _repository.Object;
    }
}