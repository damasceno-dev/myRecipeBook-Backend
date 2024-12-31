using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.ResetPassword;

public class UserResetPasswordUseCase(IUsersRepository usersRepository, PasswordEncryption passwordEncryption, IUnitOfWork unitOfWork)
{
    private const int CodeExpirationTimeInMinutes = 15;
    public async Task Execute(RequestUserResetPasswordJson request)
    {
        var user = await usersRepository.GetExistingUserWithEmail(request.Email);
        ValidateExistingUser(user);
        
        var userCodeObject = await usersRepository.GetUserResetPasswordCode(user!.Id);
        ValidateCode(userCodeObject, request.Code);
        
        ValidatePassword(request);

        user.Password = passwordEncryption.HashPassword(request.NewPassword);
        usersRepository.UpdateUser(user);

        await usersRepository.DeactivateAllPasswordCodes(user.Id);
        
        await unitOfWork.Commit();
    }

    private static void ValidatePassword(RequestUserResetPasswordJson requestUserNewPassword)
    {
        var result = new UserResetPasswordFluentValidation().Validate(requestUserNewPassword);
        if (result.IsValid is false)
        {
            throw new OnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }
    }

    private static void ValidateCode(UserPasswordResetCode? userCodeObject, string requestCode)
    {
        if (userCodeObject is null || string.IsNullOrWhiteSpace(requestCode))
        {
            throw new NotFoundException(ResourceErrorMessages.USER_PASSWORD_RESET_INVALID_CODE);
        }

        if (userCodeObject.Code.Equals(requestCode) is false)
        {
            throw new OnValidationException([ResourceErrorMessages.USER_PASSWORD_RESET_WRONG_CODE]);
        }

        if (userCodeObject.CreatedOn.AddMinutes(CodeExpirationTimeInMinutes) < DateTime.UtcNow)
        {
            throw new OnValidationException([ResourceErrorMessages.USER_PASSWORD_RESET_EXPIRED_CODE]);
        }
    }

    private static void ValidateExistingUser(User? user)
    {
        if (user is null)
        {
            throw new NotFoundException(ResourceErrorMessages.EMAIL_NOT_REGISTERED);
        }
    }
}