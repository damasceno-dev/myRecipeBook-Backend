using AutoMapper;
using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Domain.Interfaces;
using Xunit;

namespace UseCases.Test.UseCases.Users.Register;

public class RegisterUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var request = RequestUserRegisterJsonBuilder.Build();
        var useCase = CreateUserRegisterUseCase();
        var response = await useCase.Execute(request);

        response.Should().NotBeNull();
        response.Name.Should().Be(request.Name);
    }

    private UserRegisterUseCase CreateUserRegisterUseCase()
    {
        var usersRepository = UserRepositoryBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var password = PasswordEncryptionBuilder.Build();
        var mapper = MapperBuilder.Build();
        return new UserRegisterUseCase(usersRepository, unitOfWork, mapper, password);
    }
}