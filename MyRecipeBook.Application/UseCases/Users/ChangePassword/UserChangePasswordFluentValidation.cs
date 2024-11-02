using FluentValidation;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.ChangePassword;

public class UserChangePasswordFluentValidation : AbstractValidator<RequestUserChangePasswordJson>
{
    public UserChangePasswordFluentValidation()
    {
        RuleFor(u => u.NewPassword.Length).GreaterThan(6).WithMessage(ResourceErrorMessages.PASSWORD_LENGTH);
        RuleFor(u => u.NewPassword).NotEmpty().WithMessage(ResourceErrorMessages.PASSWORD_EMPTY);
        RuleFor(u => u.CurrentPassword).NotEmpty().WithMessage(ResourceErrorMessages.PASSWORD_EMPTY);
    }
}