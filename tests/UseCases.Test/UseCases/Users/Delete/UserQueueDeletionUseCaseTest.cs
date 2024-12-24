using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Services;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Delete;
using MyRecipeBook.Domain.Entities;
using Xunit;

namespace UseCases.Test.UseCases.Users.Delete;

public class UserQueueDeletionUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        var act = async () => await useCase.Execute();

        await act.Should().NotThrowAsync();

        user.Active.Should().BeFalse();
    }

    private static UserQueueDeletionUseCase CreateUseCase(User user)
    {
        var queue = DeleteUserQueueBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();

        return new UserQueueDeletionUseCase(usersRepository, unitOfWork, queue);
    }
}