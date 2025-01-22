using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.ExternalLogin;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.LoginExternal;

public class UserExternalLoginUseCaseTest
{
    [Fact]
    public async Task Success_User_Dont_Exist()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUserExternalLoginUseCase();

        var result = await useCase.Execute(name: user.Name, email: user.Email);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Success_Exist()
    {
        var (user, _) = UserBuilder.Build();

        var useCase = CreateUserExternalLoginUseCase(user);

        var result = await useCase.Execute(name: user.Name, email: user.Email);

        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task ErrorEmailNotActive()
    {
        var (user, _) = UserBuilder.Build();
        user.Active = false;
        var useCase = CreateUserExternalLoginUseCase(user);
        Func<Task> act = () => useCase.Execute(user.Name, user.Email);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
    }

    private static UserExternalLoginUseCase CreateUserExternalLoginUseCase(User? user = null)
    {

        var usersRepositoryBuilder = new UserRepositoryBuilder();
        var token = JsonWebTokenRepositoryBuilder.Build();
        var refreshToken = new RefreshTokenRepositoryBuilder().Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var passwordEncryption = PasswordEncryptionBuilder.Build();

        if (user is not null)
        { 
            usersRepositoryBuilder.GetExistingUserWithEmail(user);
        }

        var usersRepository = usersRepositoryBuilder.Build();

        return new UserExternalLoginUseCase(usersRepository, unitOfWork, token, refreshToken, passwordEncryption);
    }
}