using Microsoft.Extensions.Options;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Login;

public class UserLoginUseCase
{
    private readonly IUsersRepository _repository;
    private readonly PasswordEncryption _passwordEncryption;

    public UserLoginUseCase(IUsersRepository repository, PasswordEncryption passwordEncryption)
    {
        _repository = repository;
        _passwordEncryption = passwordEncryption;
    }
    public async Task<ResponseUserLoginJson> Execute(RequestUserLoginJson request)
    {
        Validate(request);
        var userToVerify = await _repository.GetExistingUserWithEmail(request.Email);
        var verifiedUser = VerifyUser(userToVerify, request.Password);
        
        return new ResponseUserLoginJson
        {
            Email = verifiedUser.Email,
            Name = verifiedUser.Name
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
            throw new InvalidLoginException(ResourceErrorMessages.WRONG_PASSWORD);
        }

        return validUser;
    }

    private void Validate(RequestUserLoginJson request)
    {
        var result = new UserLoginFluentValidation().Validate(request);
        if (result.IsValid is false)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errorMessages);
        }
    }
}