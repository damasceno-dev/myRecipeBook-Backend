using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Repositories;

public class UserRepositoryBuilder
{
    private readonly Mock<IUsersRepository> _repository;

    public UserRepositoryBuilder()
    {
        _repository = new Mock<IUsersRepository>();
    }
    public UserRepositoryBuilder GetExistingUserWithEmail(User? user)
    {
        _repository.Setup(u => u.GetExistingUserWithEmail(It.IsAny<string>())).ReturnsAsync(user);
        return this;
    }
    public UserRepositoryBuilder GetExistingUserWithId(User user)
    {
        _repository.Setup(u => u.GetExistingUserWithId(It.IsAny<Guid>())).ReturnsAsync(user);
        return this;
    }

    public UserRepositoryBuilder GetExistingUserWithIdAsNoTracking(User? user)
    {
        _repository.Setup(u => u.GetExistingUserWithIdAsNoTracking(It.IsAny<Guid>())).ReturnsAsync(user);
        return this;
    }

    public UserRepositoryBuilder ExistsActiveUserWithEmail(bool exists)
    {
        _repository.Setup(u => u.ExistsActiveUserWithEmail(It.IsAny<string>())).ReturnsAsync(exists);
        return this;
    }
    public UserRepositoryBuilder GetLoggedUserWithToken(User user)
    {
        _repository.Setup(u => u.GetLoggedUserWithToken()).ReturnsAsync(user);
        return this;
    }

    public IUsersRepository Build()
    {
        return _repository.Object;
    }
}