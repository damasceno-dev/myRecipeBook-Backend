using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IStorageService
{
    Task Upload(User user, Stream file, string fileName);
    Task<string> GetFileUrl(User user, string fileName);
    Task Delete(User user, string fileName);
    Task DeleteContainer(Guid userIdentifier);
}