using Microsoft.AspNetCore.Hosting;
using Moq;

namespace CommonTestUtilities.Repositories;

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
