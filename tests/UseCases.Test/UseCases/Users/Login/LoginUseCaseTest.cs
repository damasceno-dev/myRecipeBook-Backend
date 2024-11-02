using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Moq;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.Login;

public class LoginUseCaseTest
{
    private enum TestCondition
    {
        ValidLogin,
        InactiveUser,
        WrongPassword
    }
    
    [Fact]
    public async Task Success()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(request, TestCondition.ValidLogin);
        var response = await useCase.Execute(request);
        
        response.Should().NotBeNull();
        response.Email.Should().Be(request.Email);
        response.Name.Should().Be("Valid User");
        response.ResponseToken.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ErrorEmailNotRegistered()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(request);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.EMAIL_NOT_REGISTERED);
    }
    
    [Fact]
    public async Task ErrorEmailNotActive()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(request, TestCondition.InactiveUser);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
    }

    [Fact]
    public async Task ErrorWrongPassword()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        var useCase = CreateUserLoginUseCase(request, TestCondition.WrongPassword);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<InvalidLoginException>()
            .WithMessage(ResourceErrorMessages.PASSWORD_WRONG);
    }

    [Fact]
    public async Task ErrorEmptyEmail()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = " ";
        var useCase = CreateUserLoginUseCase(request);
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_NOT_EMPTY));
    }

    [Fact]
    public async Task ErrorInvalidEmail()
    {
        var request = RequestUserLoginJsonBuilder.Build();
        request.Email = "invalid_email.com";
        var useCase = CreateUserLoginUseCase(request);
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_INVALID));
    }

    private static UserLoginUseCase CreateUserLoginUseCase(RequestUserLoginJson request, TestCondition? condition = null)
    {
        var password = PasswordEncryptionBuilder.Build();
        var token = JsonWebTokenRepositoryBuilder.Build();
        var usersRepository = MockTestsConditions(request, condition, password);
        return new UserLoginUseCase(usersRepository, token, password);
    }

    private static IUsersRepository MockTestsConditions(RequestUserLoginJson request, TestCondition? condition, PasswordEncryption password)
    {
        switch (condition)
        {
            case TestCondition.ValidLogin:
                var validUser = new User {Name = "Valid User",Email = request.Email,Password = password.HashPassword(request.Password)};
                return new UserRepositoryBuilder().GetExistingUserWithEmail(validUser).Build();
            case TestCondition.InactiveUser:
                var inactiveUser = new User {Name = "Inactive User",Email = request.Email,Password = password.HashPassword(request.Password), Active = false};
                return new UserRepositoryBuilder().GetExistingUserWithEmail(inactiveUser).Build();
            case TestCondition.WrongPassword:
                var wrongPassword = new User {Name = "Wrong Password User",Email = request.Email,Password = password.HashPassword("wrong pass")};
                return new UserRepositoryBuilder().GetExistingUserWithEmail(wrongPassword).Build();
            default:
                return new UserRepositoryBuilder().Build();
        }
    }
}