using Microsoft.Extensions.Options;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
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
        var userEmail = request.Email;
        var validUser = await _repository.GetExistingUserWithEmail(userEmail);
        
        if (validUser is null)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_REGISTERED);
        } 
        
        if (validUser.Active is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
        }
        
        if (_passwordEncryption.VerifyPassword(request.Password, validUser.Password) is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.WRONG_PASSWORD);
        }
        
        return new ResponseUserLoginJson
        {
            Email = validUser.Email,
            Name = validUser.Name
        };
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