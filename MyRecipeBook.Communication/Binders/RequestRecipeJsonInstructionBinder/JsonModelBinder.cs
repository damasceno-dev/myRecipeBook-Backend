using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;

public class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
{
    if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

    var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
    if (valueProviderResult == ValueProviderResult.None)
    {
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }

    try
    {
        var rawValue = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // Normalize the value
        var normalizedValue = rawValue
            .Replace("\\n", "")         // Remove newline escapes
            .Replace("\\\"", "\"")     // Unescape quotes
            .Trim();                   // Remove leading/trailing spaces

        // Preprocess the JSON string to handle mixed object types
        var sanitizedValue = FixImproperlyQuotedJsonObjects(normalizedValue);

        // Deserialize the corrected JSON array
        var parsedInstructions = JsonSerializer.Deserialize<List<RequestRecipeInstructionJson>>(sanitizedValue);

        bindingContext.Result = ModelBindingResult.Success(parsedInstructions);
    }
    catch (Exception ex)
    {
        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid JSON format: {ex.Message}");
        bindingContext.Result = ModelBindingResult.Failed();
    }

    return Task.CompletedTask;
}
    private string FixImproperlyQuotedJsonObjects(string input)
    {
        // Regex to find improperly quoted JSON objects
        var regex = new Regex(@"(?<=\[|,)\s*""\{.*?\}""\s*(?=,|\])");
    
        // Replace the improperly quoted JSON objects with properly formatted JSON
        var sanitizedInput = regex.Replace(input, match =>
        {
            var trimmedMatch = match.Value.Trim('\"'); // Remove the surrounding quotes
            return trimmedMatch; // Return the unquoted JSON object
        });

        return sanitizedInput;
    }

}