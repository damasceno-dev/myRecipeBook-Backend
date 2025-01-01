using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Services;
using MyRecipeBook.Application.UseCases.Users.ResetPassword;
using MyRecipeBook.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace UseCases.Test.UseCases.Users.ResetPassword;

public class UserGetResetPasswordCodeUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUserGetResetPasswordCodeUseCase(user);

        var act = () => useCase.Execute(user.Email);

        await act.Should().NotThrowAsync();
    }
    
    private static UserGetResetPasswordCodeUseCase CreateUserGetResetPasswordCodeUseCase(User user)
    {

        var usersRepository = new UserRepositoryBuilder().GetExistingUserWithEmail(user).Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var senEmailRepository = SendUserResetPasswordCodeBuilder.Build();

        return new UserGetResetPasswordCodeUseCase(usersRepository, unitOfWork, senEmailRepository);
    }
}