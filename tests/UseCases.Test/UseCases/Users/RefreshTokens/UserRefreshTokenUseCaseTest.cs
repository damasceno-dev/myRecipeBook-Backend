using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Token;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Users.RefreshTokens;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Users.RefreshTokens;

public class UserRefreshTokenUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var refreshToken = RefreshTokenBuilder.Build();
        var useCase = CreateUserRefreshTokenUseCase(refreshToken);
        var request = new RequestRefreshTokenJson { RefreshToken = refreshToken.Value };
        
        var response = await useCase.Execute(request);
        
        response.Should().NotBeNull();
        response.Token.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task ErrorInvalidToken()
    {
        var refreshToken = RefreshTokenBuilder.Build();
        var useCase = CreateUserRefreshTokenUseCase();
        var request = new RequestRefreshTokenJson { RefreshToken = refreshToken.Value };
        
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<RefreshTokenInvalidException>()
            .WithMessage(ResourceErrorMessages.REFRESH_TOKEN_INVALID);
    }
    
    [Fact]
    public async Task ErrorExpiredToken()
    {
        var refreshToken = RefreshTokenBuilder.Build(expired: true);
        var useCase = CreateUserRefreshTokenUseCase(refreshToken);
        var request = new RequestRefreshTokenJson { RefreshToken = refreshToken.Value };
        
        Func<Task> act = () => useCase.Execute(request);
        
        await act.Should().ThrowAsync<RefreshTokenExpiredException>()
            .WithMessage(ResourceErrorMessages.REFRESH_TOKEN_EXPIRED);
    }
    
    private static UserRefreshTokenUseCase CreateUserRefreshTokenUseCase(RefreshToken? rtFromRequest = null)
    {
        var tokenRepository = JsonWebTokenRepositoryBuilder.Build();
        var refreshTokenRepository = new RefreshTokenRepositoryBuilder().Generate(RefreshTokenBuilder.GenerateTestRefreshToken()).GetRefreshToken(rtFromRequest).Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        return new UserRefreshTokenUseCase(tokenRepository, refreshTokenRepository, unitOfWork);
    }
}