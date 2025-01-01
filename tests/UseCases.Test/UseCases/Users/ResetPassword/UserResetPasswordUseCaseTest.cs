using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Users.ResetPassword;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.ResetPassword;

public class UserResetPasswordUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserResetPasswordJsonBuilder.Build();
        var userCode = UserPasswordCodeBuilder.Build(user.Id, request.Code); 
        var useCase = CreateUserResetPasswordUseCase(user, userCode);

        var act = () => useCase.Execute(request);

        await act.Should().NotThrowAsync();
    }
    [Fact]
    public async Task UserNullError()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserResetPasswordJsonBuilder.Build();
        var userCode = UserPasswordCodeBuilder.Build(user.Id, request.Code); 
        var useCase = CreateUserResetPasswordUseCase(user: null, userCode);

        var act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(ResourceErrorMessages.EMAIL_NOT_REGISTERED);
    }

    [Fact]
    public async Task UserPasswordCodeNullError()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserResetPasswordJsonBuilder.Build();
        var useCase = CreateUserResetPasswordUseCase(user, userCode: null);

        var act = () => useCase.Execute(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(ResourceErrorMessages.USER_PASSWORD_RESET_INVALID_CODE);
    }

    [Fact]
    public async Task WrongCodeError()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserResetPasswordJsonBuilder.Build();
        var wrongCode = DigitGenerator.Generate6DigitCode();
        while (wrongCode == request.Code)
        {
            wrongCode = DigitGenerator.Generate6DigitCode();
        }
        var userCode = UserPasswordCodeBuilder.Build(user.Id, wrongCode); 
        var useCase = CreateUserResetPasswordUseCase(user, userCode);

        var act = () => useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.USER_PASSWORD_RESET_WRONG_CODE));
    }
    
    [Fact]
    public async Task ExpiredCodeError()
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserResetPasswordJsonBuilder.Build();
        var expiredCode = UserPasswordCodeBuilder.Build(user.Id, request.Code);
        const int expiredTime = UserResetPasswordUseCase.CodeExpirationTimeInMinutes + 1;
        expiredCode.CreatedOn = DateTime.UtcNow.AddMinutes(-expiredTime); 
        var useCase = CreateUserResetPasswordUseCase(user, expiredCode);

        var act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.USER_PASSWORD_RESET_EXPIRED_CODE));
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task  ErrorPasswordInvalid(int passwordLength)
    {
        var (user, _) = UserBuilder.Build();
        var request = RequestUserResetPasswordJsonBuilder.Build(passwordLength);
        var userCode = UserPasswordCodeBuilder.Build(user.Id, request.Code); 
        var useCase = CreateUserResetPasswordUseCase(user, userCode);

        var act = () => useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_LENGTH));
    }

    private static UserResetPasswordUseCase CreateUserResetPasswordUseCase(User? user, UserPasswordResetCode? userCode)
    {
        var usersRepository = new UserRepositoryBuilder().GetExistingUserWithEmail(user).GetUserResetPasswordCode(userCode).Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var password = PasswordEncryptionBuilder.Build();

        return new UserResetPasswordUseCase(usersRepository, password, unitOfWork);
    }
}