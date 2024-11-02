using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.ChangePassword;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.ChangePassword;

public class UserChangePasswordUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(7);
        var user = new User()
        {
            Id = Guid.NewGuid(), Name = "John Doe", Email = "john.doe@example.com", 
            Password = PasswordEncryptionBuilder.Build().HashPassword(request.CurrentPassword),
        };
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ErrorNewPasswordEmpty()
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(7);
        request.NewPassword = string.Empty;
        var user = new User()
        {
            Id = Guid.NewGuid(), Name = "John Doe", Email = "john.doe@example.com", 
            Password = PasswordEncryptionBuilder.Build().HashPassword(request.CurrentPassword),
        };
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_EMPTY));
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task ErrorNewPasswordLength(int passwordLength)
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(passwordLength);
        var user = new User()
        {
            Id = Guid.NewGuid(), Name = "John Doe", Email = "john.doe@example.com", 
            Password = PasswordEncryptionBuilder.Build().HashPassword(request.CurrentPassword),
        };
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e =>e.GetErrors.Count == 1 && e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_LENGTH));
    }
    
    [Fact]
    public async Task ErrorPasswordWrong()
    {
        var request = RequestUserChangePasswordJsonBuilder.Build(7);
        var user = new User()
        {
            Id = Guid.NewGuid(), Name = "John Doe", Email = "john.doe@example.com", 
            Password = PasswordEncryptionBuilder.Build().HashPassword("wrong_password"),
        };
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e =>e.GetErrors.Count == 1 && e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_WRONG));
    }
    

    private static UserChangePasswordUseCase CreateUserChangePasswordUseCase(User user)
    {
        var tokenProvider = new JsonWebTokenProviderBuilder().Build();
        var tokenRepository = new TokenRepositoryBuilder().ValidateAndGetUserIdentifier(user.Id).Build();
        var usersRepository = new UserRepositoryBuilder().GetExistingUserWithId(user).Build();
        var passwordEncryption = PasswordEncryptionBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        return new UserChangePasswordUseCase(tokenProvider, tokenRepository, usersRepository, passwordEncryption, 
            unitOfWork);
    }
}