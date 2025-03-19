using FluentValidation;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Users.Update;

public class UserUpdateFluentValidation : AbstractValidator<RequestUserUpdateJson>
{
    public UserUpdateFluentValidation()
    {
        // At least one field must be provided
        RuleFor(u => u)
            .Must(u => !string.IsNullOrWhiteSpace(u.Name) || !string.IsNullOrWhiteSpace(u.Email))
            .WithMessage("At least one field (name or email) must be provided for update");

        // Validate name only if it's provided
        When(u => !string.IsNullOrWhiteSpace(u.Name), () =>
        {
            RuleFor(u => u.Name).NotEmpty().WithMessage(ResourceErrorMessages.NAME_NOT_EMPTY);
        });

        // Validate email format only if it's provided
        When(u => !string.IsNullOrWhiteSpace(u.Email), () =>
        {
            RuleFor(u => u.Email).EmailAddress().WithMessage(ResourceErrorMessages.EMAIL_INVALID);
        });
    }
}