using Moq;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Repositories;

public static class UserRepositoryBuilder
{
    public static IUsersRepository Build()
    {
        return new Mock<IUsersRepository>().Object;
    }
}