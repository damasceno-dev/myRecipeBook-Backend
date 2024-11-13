using CommonTestUtilities.Entities;
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
        var (user, _) = UserBuilder.Build();
        var useCase = CreateUserProfileWithTokenUseCase(user);
        
        var response = await useCase.Execute();
        
        response.Should().NotBeNull();
        response.Email.Should().Be(user.Email);
        response.Name.Should().Be(user.Name);
    }
    
    [Fact]
    public async Task InactiveUser()
    {
        var (inactiveUser, _) = UserBuilder.Build();
        inactiveUser.Active = false;
        var useCase = CreateUserProfileWithTokenUseCase(inactiveUser);
        Func<Task> act = () => useCase.Execute();
        
        await act.Should().ThrowAsync<InvalidLoginException>().WithMessage(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
    }
    private static UserProfileWithTokenUseCase CreateUserProfileWithTokenUseCase(User user)
    {
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var mapper = MapperBuilder.Build();
        return new UserProfileWithTokenUseCase(usersRepository, mapper);
    }
}