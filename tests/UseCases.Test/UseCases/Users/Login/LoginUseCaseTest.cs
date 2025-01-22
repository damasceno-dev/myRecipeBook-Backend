using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.Login;

public class LoginUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, password) = UserBuilder.Build();
        var request = RequestUserLoginJsonBuilder.Build();
        request.Password = password;
        var useCase = CreateUserLoginUseCase(user);
        var response = await useCase.Execute(request);
        
        response.Should().NotBeNull();
        response.Email.Should().Be(user.Email);
        response.Name.Should().Be(user.Name);
        response.ResponseToken.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ErrorEmailNotRegistered()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(null);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.EMAIL_NOT_REGISTERED);
    }
    
    [Fact]
    public async Task ErrorEmailNotActive()
    {
        var (user, _) = UserBuilder.Build();
        user.Active = false;
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(user);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
    }

    [Fact]
    public async Task ErrorWrongPassword()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(user);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.PASSWORD_WRONG);
    }

    [Fact]
    public async Task ErrorEmptyEmail()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = " ";
        var useCase = CreateUserLoginUseCase(user);
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_NOT_EMPTY));
    }

    [Fact]
    public async Task ErrorInvalidEmail()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = "invalid_email.com";
        var useCase = CreateUserLoginUseCase(user);
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_INVALID));
    }
    private static UserLoginUseCase CreateUserLoginUseCase(User? user)
    {
        var usersRepository = new UserRepositoryBuilder().GetExistingUserWithEmail(user).Build();
        var token = JsonWebTokenRepositoryBuilder.Build();
        var refreshToken = new RefreshTokenRepositoryBuilder().Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var passwordEncryption = PasswordEncryptionBuilder.Build();
        return new UserLoginUseCase(usersRepository, token, refreshToken, unitOfWork, passwordEncryption);
    }
}