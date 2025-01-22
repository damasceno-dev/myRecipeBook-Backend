using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.ExternalLogin;

public class UserExternalLoginUseCase(IUsersRepository usersRepository, IUnitOfWork unitOfWork, ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, PasswordEncryption passwordEncryption)
{
    public async Task<ResponseUserLoginJson> Execute(string name, string email)
    {
        var user = await usersRepository.GetExistingUserWithEmail(email);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Password = passwordEncryption.GetDefaultExternalLoginKey()
            };

            await usersRepository.Register(user);
        }
        
        if (user.Active is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
        }
        
        var refreshToken = new RefreshToken { Value = refreshTokenRepository.Generate(), UserId = user.Id };
        await refreshTokenRepository.SaveRefreshToken(refreshToken);
        await unitOfWork.Commit();
        var tokenResponse = new ResponseTokenJson
        {
            Token = tokenRepository.Generate(user.Id),
            RefreshToken = refreshToken.Value
        };
        return new ResponseUserLoginJson { Email = user.Email, Name = user.Name, ResponseToken = tokenResponse };
    }
}