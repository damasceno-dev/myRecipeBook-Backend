using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Services;

public class StorageServiceBuilder
{
    private readonly Mock<IStorageService> _service = new();

    public StorageServiceBuilder Upload()
    {
        _service.Setup(service => service.Upload(It.IsAny<User>(), It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return this;
    }
    public StorageServiceBuilder Delete()
    {
        _service.Setup(service => service.Delete(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        return this;
    }
    
    public StorageServiceBuilder GetFileUrl(IEnumerable<Recipe> recipes)
    {
        const string baseUrl = "https://example.com/";
        _service.Setup(service => service.GetFileUrl(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User user, string fileName) =>
            {
                var recipe = recipes.FirstOrDefault(r => r.ImageIdentifier == fileName);
                if (recipe != null && !string.IsNullOrWhiteSpace(recipe.ImageIdentifier))
                {
                    return $"{baseUrl}{recipe.ImageIdentifier}";
                }
                return string.Empty; 
            });
        return this;
    }
    public StorageServiceBuilder GetFileUrl()
    {
        const string baseUrl = "https://example.com/";
        _service.Setup(service => service.GetFileUrl(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User user, string fileName) => $"{baseUrl}{fileName}");
        return this;
    }

    public IStorageService Build()
    {
        return _service.Object;
    }
}