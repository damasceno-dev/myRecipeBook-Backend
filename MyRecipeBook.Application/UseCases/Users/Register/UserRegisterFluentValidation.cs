using FluentValidation;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.Register;

public class UserRegisterFluentValidation : AbstractValidator<RequestUserRegisterJson>
{
    public UserRegisterFluentValidation()
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage(ResourceErrorMessages.NAME_NOT_EMPTY);
        RuleFor(u => u.Email).NotEmpty().WithMessage(ResourceErrorMessages.EMAIL_NOT_EMPTY);
        RuleFor(u => u.Password).ValidatePassword();
        When(u => !string.IsNullOrWhiteSpace(u.Email), () =>
            RuleFor(u => u.Email).EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_INVALID));
    }
}