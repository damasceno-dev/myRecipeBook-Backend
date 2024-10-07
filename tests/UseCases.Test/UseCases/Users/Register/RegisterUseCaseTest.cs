using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
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
    }

    [Fact]
    public async Task EmailAlreadyExistsError()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var useCase = CreateUserRegisterUseCase(TestCondition.EmailAlreadyExists);
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<ConflictException>().WithMessage($"{request.Email} - {ResourceErrorMessages.EMAIL_ALREADY_EXISTS}");
    }
    private UserRegisterUseCase CreateUserRegisterUseCase(TestCondition? condition = null)
    {
        var usersRepository = UserRepositoryBuilder.Build();
        if (condition == TestCondition.EmailAlreadyExists)
        {
            usersRepository.Setup(u => u.ExistsActiveUserWithEmail(It.IsAny<string>())).ReturnsAsync(true);
        }
        var unitOfWork = UnitOfWorkBuilder.Build();
        var password = PasswordEncryptionBuilder.Build();
        var mapper = MapperBuilder.Build();
        return new UserRegisterUseCase(usersRepository.Object, unitOfWork, mapper, password);
    }
}