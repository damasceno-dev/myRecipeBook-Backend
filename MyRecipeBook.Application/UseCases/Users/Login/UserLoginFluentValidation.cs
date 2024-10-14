using FluentValidation;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.Login;

public class UserLoginFluentValidation : AbstractValidator<RequestUserLoginJson>
{
    public UserLoginFluentValidation()
    {
        RuleFor(r => r.Email).NotEmpty().WithMessage(ResourceErrorMessages.EMAIL_NOT_EMPTY);
        When(r => !string.IsNullOrWhiteSpace(r.Email), () =>
        {
            RuleFor(u => u.Email).EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_INVALID);
        });
    }
}