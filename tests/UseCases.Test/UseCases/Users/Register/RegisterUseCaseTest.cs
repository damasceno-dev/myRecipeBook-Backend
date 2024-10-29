using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Moq;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Communication;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.Register;

public class RegisterUseCaseTest
{
    private enum TestCondition
    {
        EmailAlreadyExists
    }
    [Fact]
    public async Task Success()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var useCase = CreateUserRegisterUseCase();
        var response = await useCase.Execute(request);

        response.Should().NotBeNull();
        response.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.ResponseToken.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EmailAlreadyExistsError()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var useCase = CreateUserRegisterUseCase(TestCondition.EmailAlreadyExists);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"{request.Email} - {ResourceErrorMessages.EMAIL_ALREADY_EXISTS}");
    }
    
    [Fact]
    public async Task UserNameIsEmptyError()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        request.Name = " ";
        var useCase = CreateUserRegisterUseCase();
        Func<Task> act = () => useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.NAME_NOT_EMPTY));
    }    
    
    [Fact]
    public async Task InvalidNameEmailAndPasswordError()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        request.Name = " ";
        request.Email = " ";
        request.Password = "1234";
        var useCase = CreateUserRegisterUseCase();
        Func<Task> act = () => useCase.Execute(request);

        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 3 && 
                        e.GetErrors.Contains(ResourceErrorMessages.NAME_NOT_EMPTY) &&
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_NOT_EMPTY) &&
                        e.GetErrors.Contains(ResourceErrorMessages.PASSWORD_LENGTH));
    }
    
    private static UserRegisterUseCase CreateUserRegisterUseCase(TestCondition? condition = null)
    {
        var usersRepository = UserRepositoryBuilder.Build();
        if (condition == TestCondition.EmailAlreadyExists)
        {
            usersRepository.Setup(u => u.ExistsActiveUserWithEmail(It.IsAny<string>())).ReturnsAsync(true);
        }
        var unitOfWork = UnitOfWorkBuilder.Build();
        var password = PasswordEncryptionBuilder.Build();
        var mapper = MapperBuilder.Build();
        var token = JsonWebTokenRepositoryBuilder.Build();
        return new UserRegisterUseCase(usersRepository.Object, unitOfWork, mapper, token, password);
    }
}