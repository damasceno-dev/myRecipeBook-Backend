using FluentValidation;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.ResetPassword;

public class UserResetPasswordFluentValidation : AbstractValidator<RequestUserResetPasswordJson>
{
    public UserResetPasswordFluentValidation()
    {
        RuleFor(p => p.NewPassword).ValidatePassword();
    }
}