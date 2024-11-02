using Microsoft.Extensions.Options;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Login;

public class UserLoginUseCase
{
    private readonly IUsersRepository _repository;
    private readonly ITokenRepository _tokenRepository;
    private readonly PasswordEncryption _passwordEncryption;

    public UserLoginUseCase(IUsersRepository repository, ITokenRepository tokenRepository, PasswordEncryption passwordEncryption)
    {
        _repository = repository;
        _tokenRepository = tokenRepository;
        _passwordEncryption = passwordEncryption;
    }
    public async Task<ResponseUserLoginJson> Execute(RequestUserLoginJson request)
    {
        Validate(request);
        var userToVerify = await _repository.GetExistingUserWithEmail(request.Email);
        var verifiedUser = VerifyUser(userToVerify, request.Password);
        var userToken = _tokenRepository.Generate(verifiedUser.Id);
        
        return new ResponseUserLoginJson
        {
            Email = verifiedUser.Email,
            Name = verifiedUser.Name,
            ResponseToken = new ResponseTokenJson {Token = userToken}
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
        
        if (_passwordEncryption.VerifyPassword(requestPassword, validUser.Password) is false)
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