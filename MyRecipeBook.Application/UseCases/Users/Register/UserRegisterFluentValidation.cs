using FluentValidation;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.Register;

public class UserRegisterFluentValidation : AbstractValidator<RequestUserRegisterJson>
{
    public UserRegisterFluentValidation()
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage(ResourceErrorMessages.NAME_NOT_EMPTY);
        RuleFor(u => u.Email).NotEmpty().WithMessage(ResourceErrorMessages.EMAIL_NOT_EMPTY);
        RuleFor(u => u.Password.Length).GreaterThan(6).WithMessage(ResourceErrorMessages.PASSWORD_LENGTH);
        When(u => !string.IsNullOrEmpty(u.Email), () =>
            RuleFor(u => u.Email).EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_INVALID));
    }
}