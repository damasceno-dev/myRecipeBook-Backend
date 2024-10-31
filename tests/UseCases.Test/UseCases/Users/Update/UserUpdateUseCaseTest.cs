using Bogus.DataSets;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;
using Moq;
using MyRecipeBook.Application.UseCases.Users.Profile;
using MyRecipeBook.Application.UseCases.Users.Update;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.Update;

public class UserUpdateUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var currentUser = new User() { Name = "currentUser", Email = "currentEmail@email.com" };
        var request = RequestUserUpdateJsonBuilder.Build();
        var useCase = CreateUserUpdateUseCase(currentUser);
        var response = await useCase.Execute(request);
        
        response.Should().NotBeNull();
        response.Email.Should().Be(request.Email);
        response.Name.Should().Be(request.Name);
    }
    
    [Fact]
    public async Task ErrorEmailEmpty()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        request.Email = string.Empty;
        var useCase = CreateUserUpdateUseCase();
        
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_NOT_EMPTY));
    }
    [Fact]
    public async Task ErrorEmailInvalid()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        request.Email = "invalid_email_example";
        var useCase = CreateUserUpdateUseCase();
        
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.EMAIL_INVALID));
    }
    
    [Fact]
    public async Task ErrorNameEmpty()
    {
        var request = RequestUserUpdateJsonBuilder.Build();
        request.Name = string.Empty;
        var useCase = CreateUserUpdateUseCase();
        
        Func<Task> act = () => useCase.Execute(request);
        
        (await act.Should().ThrowAsync<OnValidationException>())
            .Where(e => e.GetErrors.Count == 1 && 
                        e.GetErrors.Contains(ResourceErrorMessages.NAME_NOT_EMPTY));
    }
    
    [Fact]
    public async Task EmailAlreadyExists()
    {
        var currentUser = new User() { Name = "currentUser", Email = "currentEmail@email.com" };
        var request = RequestUserUpdateJsonBuilder.Build();
        var useCase = CreateUserUpdateUseCase(currentUser, true);
        
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ResourceErrorMessages.EMAIL_ALREADY_EXISTS);
    }
    
    private static UserUpdateUseCase CreateUserUpdateUseCase(User? user = null, bool emailAlreadyExists = false)
    {
        var tokenProvider = JsonWebTokenProviderBuilder.Build();
        var tokenRepository = TokenRepositoryBuilder.Build();
        var usersRepositoryMock = UserRepositoryBuilder.Build();
        if (user is not null)
        {
            usersRepositoryMock.Setup(u => u.GetExistingUserWithId(It.IsAny<Guid>())).ReturnsAsync(user);
        }
        if (emailAlreadyExists)
        {
            usersRepositoryMock.Setup(u => u.GetExistingUserWithEmail(It.IsAny<string>())).ReturnsAsync(user);
        }
        var mapper = MapperBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        return new UserUpdateUseCase(usersRepositoryMock.Object, tokenProvider, tokenRepository, unitOfWork, mapper);
    }
}