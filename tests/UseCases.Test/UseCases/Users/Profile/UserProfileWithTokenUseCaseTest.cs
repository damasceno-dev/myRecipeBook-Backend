using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Token;
using FluentAssertions;
using Moq;
using MyRecipeBook.Application.UseCases.Users.Profile;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.Profile;

public class UserProfileWithTokenUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var validUser = new User {Name = "Valid user", Email = "valid@example.com"};
        var useCase = CreateUserProfileWithTokenUseCase(validUser);
        var response = await useCase.Execute();
        
        response.Should().NotBeNull();
        response.Email.Should().Be(validUser.Email);
        response.Name.Should().Be(validUser.Name);
    }
    
    [Fact]
    public async Task InactiveUser()
    {
        var inactiveUser = new User {Name = "Valid user", Email = "valid@example.com", Active = false};
        var useCase = CreateUserProfileWithTokenUseCase(inactiveUser);
        Func<Task> act = () => useCase.Execute();
        
        await act.Should().ThrowAsync<InvalidLoginException>().WithMessage(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
    }
    private static UserProfileWithTokenUseCase CreateUserProfileWithTokenUseCase(User? mockUser = null)
    {
        var usersRepositoryBuilder = new UserRepositoryBuilder();
        if (mockUser is not null)
        {
            usersRepositoryBuilder.GetLoggedUserWithToken(mockUser);
        }
        var usersRepository = usersRepositoryBuilder.Build();
        var mapper = MapperBuilder.Build();
        return new UserProfileWithTokenUseCase(usersRepository, mapper);
    }
}