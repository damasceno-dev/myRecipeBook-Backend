using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Services;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Delete;
using Xunit;

namespace UseCases.Test.UseCases.Users.Delete;

public class UserDeleteUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase();

        var act = async () => await useCase.Execute(user.Id);

        await act.Should().NotThrowAsync();
    }

    private static UserDeleteUseCase CreateUseCase()
    {
        var unitOfWork = UnitOfWorkBuilder.Build();
        var storageService = new StorageServiceBuilder().Build();
        var usersRepository = new UserRepositoryBuilder().Build();

        return new UserDeleteUseCase(usersRepository, storageService, unitOfWork);
    }
}