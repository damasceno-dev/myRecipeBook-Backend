using Moq;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Repositories;

public static class UserRepositoryBuilder
{
    public static Mock<IUsersRepository> Build()
    {
        return new Mock<IUsersRepository>();
    }
}