using FluentValidation;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.Update;

public class UserUpdateFluentValidation : AbstractValidator<RequestUserUpdateJson>
{
    public UserUpdateFluentValidation()
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage(ResourceErrorMessages.NAME_NOT_EMPTY);
        RuleFor(u => u.Email).NotEmpty().WithMessage(ResourceErrorMessages.EMAIL_NOT_EMPTY);
        When(u => string.IsNullOrWhiteSpace(u.Email) is false, () =>
        {
            RuleFor(u => u.Email).EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_INVALID);
        });
    }
}