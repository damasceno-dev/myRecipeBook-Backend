using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.RefreshTokens;

public class UserRefreshTokenUseCase(ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
{
    public async Task<ResponseTokenJson> Execute(RequestRefreshTokenJson request)
    {
        var refreshTokenEntity = await refreshTokenRepository.GetRefreshToken(request.RefreshToken);
        if (refreshTokenEntity is null)
        {
            throw new RefreshTokenInvalidException();
        }

        if (refreshTokenEntity.CreatedOn.AddDays(SharedValidators.RefreshTokenExpirationTimeInDays) < DateTime.UtcNow)
        {
            throw new RefreshTokenExpiredException();
        }

        var newRefreshToken = new RefreshToken
        {
            Value = refreshTokenRepository.Generate(),
            UserId = refreshTokenEntity.UserId
        };
        
        await refreshTokenRepository.SaveRefreshToken(newRefreshToken);
        await unitOfWork.Commit();
        
        return new ResponseTokenJson
        {
            Token = tokenRepository.Generate(refreshTokenEntity.UserId),
            RefreshToken = newRefreshToken.Value
        };
    }
}