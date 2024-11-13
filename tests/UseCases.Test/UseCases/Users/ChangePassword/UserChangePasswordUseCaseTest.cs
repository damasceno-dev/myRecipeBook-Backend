using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
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
        var request = RequestUserChangePasswordJsonBuilder.Build();
        var (user, password) = UserBuilder.Build();
        request.CurrentPassword = password;
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task ErrorNewPasswordEmpty()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserChangePasswordJsonBuilder.Build();
        request.NewPassword = string.Empty;
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_EMPTY));
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
        var (user, _) = UserBuilder.Build();
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e =>e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_LENGTH));
    }
    
    [Fact]
    public async Task ErrorPasswordWrong()
    {
        var request = RequestUserChangePasswordJsonBuilder.Build();
        var (user, _) = UserBuilder.Build();
        var useCase = CreateUserChangePasswordUseCase(user);
        
        var act = async () => await useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e =>e.GetErrors.Count == 1 && e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_WRONG));
    }
    

    private static UserChangePasswordUseCase CreateUserChangePasswordUseCase(User user)
    {
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var passwordEncryption = PasswordEncryptionBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        return new UserChangePasswordUseCase(usersRepository, passwordEncryption, unitOfWork);
    }
}