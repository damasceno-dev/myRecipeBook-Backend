using FluentValidation;
using MyRecipeBook.Communication;

namespace MyRecipeBook.Application.Services;

public static class SharedValidators
{
    private const int MinimumPasswordLength = 6;
    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> password)
    {
        return password
            .NotEmpty().WithMessage(ResourceErrorMessages.PASSWORD_EMPTY)
            .Must(p => string.IsNullOrWhiteSpace(p) || p.Length >= MinimumPasswordLength)
            .WithMessage(ResourceErrorMessages.PASSWORD_LENGTH);
    }
}