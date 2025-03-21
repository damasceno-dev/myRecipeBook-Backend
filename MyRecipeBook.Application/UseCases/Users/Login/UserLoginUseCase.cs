using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Login;

public class UserLoginUseCase(IUsersRepository repository, ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, PasswordEncryption passwordEncryption)
{
    public async Task<ResponseUserLoginJson> Execute(RequestUserLoginJson request)
    {
        Validate(request);
        var userToVerify = await repository.GetExistingUserWithEmail(request.Email);
        var verifiedUser = VerifyUser(userToVerify, request.Password);
        var userToken = tokenRepository.Generate(verifiedUser.Id);
        
        var refreshToken = new RefreshToken
        {
            Value = refreshTokenRepository.Generate(),
            UserId = verifiedUser.Id
        };
        await refreshTokenRepository.SaveRefreshToken(refreshToken);
        await unitOfWork.Commit();
        
        return new ResponseUserLoginJson
        {
            Email = verifiedUser.Email,
            Name = verifiedUser.Name,
            ResponseToken = new ResponseTokenJson {Token = userToken, RefreshToken = refreshToken.Value}
        };
    }

    private User VerifyUser(User? validUser, string requestPassword)
    {
        if (validUser is null)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_REGISTERED);
        } 
        
        if (validUser.Active is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
        }

        // If this is a Google user trying to login with password, update their password
        if (validUser.Password == passwordEncryption.GetDefaultExternalLoginKey())
        {
            throw new InvalidLoginException(ResourceErrorMessages.USER_IS_ALREADY_GOOGLE);
        }
        
        if (passwordEncryption.VerifyPassword(requestPassword, validUser.Password) is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.PASSWORD_WRONG);
        }

        return validUser;
    }

    private static void Validate(RequestUserLoginJson request)
    {
        var result = new UserLoginFluentValidation().Validate(request);
        if (result.IsValid is false)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errorMessages);
        }
    }
}