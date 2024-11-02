using FluentValidation;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.ChangePassword;

public class UserChangePasswordFluentValidation : AbstractValidator<RequestUserChangePasswordJson>
{
    public UserChangePasswordFluentValidation()
    {
        RuleFor(u => u.NewPassword).ValidatePassword();
        RuleFor(u => u.CurrentPassword).NotEmpty().WithMessage(ResourceErrorMessages.PASSWORD_EMPTY);
    }
}