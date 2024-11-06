using FluentValidation.Results;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.ChangePassword;

public class UserChangePasswordUseCase
{
    private readonly IUsersRepository _usersRepository;
    private readonly PasswordEncryption _passwordEncryption;
    private readonly IUnitOfWork _unitOfWork;

    public UserChangePasswordUseCase(IUsersRepository
            usersRepository, PasswordEncryption passwordEncryption, IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _passwordEncryption = passwordEncryption;
        _unitOfWork = unitOfWork;
    }
    public async Task Execute(RequestUserChangePasswordJson request)
    {
        var user = await _usersRepository.GetLoggedUserWithToken();
        
        Validate(user, request);

        user.Password = _passwordEncryption.HashPassword(request.NewPassword);
        _usersRepository.UpdateUser(user);
        await _unitOfWork.Commit();
    }

    private void Validate(User user, RequestUserChangePasswordJson request)
    {
        var validation = new UserChangePasswordFluentValidation().Validate(request);
        if (_passwordEncryption.VerifyPassword(request.CurrentPassword, user.Password) is false)
        {
            validation.Errors.Add(new ValidationFailure()
            {
                PropertyName = nameof(RequestUserChangePasswordJson.CurrentPassword),
                ErrorMessage = ResourceErrorMessages.PASSWORD_WRONG
            });
        }
        if (validation.IsValid is false)
        {
            var errors = validation.Errors.Select(v => v.ErrorMessage).ToList();
            throw new OnValidationException(errors);
        }
    }
}