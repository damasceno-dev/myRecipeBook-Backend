using System.Text.RegularExpressions;
using FileTypeChecker.Extensions;
using FileTypeChecker.Types;
using FluentValidation;
using MyRecipeBook.Communication;

namespace MyRecipeBook.Application.Services;

public static partial class SharedValidators
{
    public const int MinimumPasswordLength = 6;
    public const int MaximumRecipeInstructionTextLength = 2000;
    public const int MaximumRecipeIngredients = 10;
    private const int MaximumRecipeIngredientWords = 5;
    
    public static (bool isValidImage, string extension) ValidateImageAndGetExtension(this Stream file)
    {
        if (file.Is<JointPhotographicExpertsGroup>())
        {
            return (true, JointPhotographicExpertsGroup.TypeExtension);
        }

        if (file.Is<PortableNetworkGraphic>())
        {
            return (true, PortableNetworkGraphic.TypeExtension);
        }

        return (false, string.Empty);
    }
    private static string? LastValidationError { get; set; }
    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> password)
    {
        return password
            .NotEmpty().WithMessage(ResourceErrorMessages.PASSWORD_EMPTY)
            .Must(p => string.IsNullOrWhiteSpace(p) || p.Length >= MinimumPasswordLength)
            .WithMessage(ResourceErrorMessages.PASSWORD_LENGTH);
    }
    
    public static IRuleBuilderOptions<T, string> ValidateIngredient<T>(this IRuleBuilder<T, string> ingredient)
    {
        return ingredient
            .Must(ingredientValue =>
            {
                if (IsValidIngredient(ingredientValue, out var errorMessage) is false)
                {
                    LastValidationError = errorMessage;
                    return false;
                }
                LastValidationError = null;
                return true;
            })
            .WithMessage(_ => LastValidationError ?? ResourceErrorMessages.RECIPE_INGREDIENT_UNEXPECTED_ERROR);
    }

    private static bool IsValidIngredient(string ingredient, out string? errorMessage)
    {
        ingredient = NormalizeSpaces(ingredient);
        
        if (string.IsNullOrWhiteSpace(ingredient))
        {
            errorMessage = ResourceErrorMessages.RECIPE_INGREDIENT_NOT_EMPTY;
            return false;
        }
        
        if (!StartsWithValidCharacter(ingredient))
        {
            errorMessage = ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_START_CHARACTER;
            return false;
        }
        
        if (ContainsInvalidSeparators(ingredient))
        {
            errorMessage = ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_SEPARATORS;
            return false;
        }
        
        if (ExceedsWordCount(ingredient))
        {
            errorMessage = ResourceErrorMessages.RECIPE_INGREDIENT_MAXIMUM_WORD_COUNT;
            return false;
        }
        
        if (ContainsInvalidCharacters(ingredient))
        {
            errorMessage = ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_CHARACTER;
            return false;
        }

        errorMessage = null;
        return true;
    }

    private static bool ContainsInvalidSeparators(string input)
    {
        var words = input.Split(' ');

        foreach (var word in words)
        {
            if (word.Contains('-') || word.Contains('_'))
                return true;

            if (word.Contains('/') && !SlashIsBetweenDigitsRegex().IsMatch(word))
                return true;
        }

        return false;
    }

    private static bool ExceedsWordCount(string input) => input.Split(' ').Length > MaximumRecipeIngredientWords;
    private static bool StartsWithValidCharacter(string input) => StartsWithValidCharacterRegex().IsMatch(input.TrimStart());
    private static bool ContainsInvalidCharacters(string input) => ContainsInvalidCharactersRegex().IsMatch(input);
    private static string NormalizeSpaces(string input) => OneOrMoreSpacesRegex().Replace(input.Trim(), " ");
    
    #region Regex
        [GeneratedRegex(@"\s+")] private static partial Regex OneOrMoreSpacesRegex();
        [GeneratedRegex(@"[^a-zA-ZÀ-ÿ0-9\s/]")] private static partial Regex ContainsInvalidCharactersRegex();
        [GeneratedRegex(@"^(\d+/\d+|\d+|[a-zA-ZÀ-ÿ])")] private static partial Regex StartsWithValidCharacterRegex();
        [GeneratedRegex(@"^\d+/\d+$")] private static partial Regex SlashIsBetweenDigitsRegex();
    #endregion
}
