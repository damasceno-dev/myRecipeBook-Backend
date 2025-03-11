using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;

public partial class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

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

            // Try to parse as array first
            try
            {
                var parsedInstructions = JsonSerializer.Deserialize<List<RequestRecipeInstructionJson>>(normalizedValue);
                bindingContext.Result = ModelBindingResult.Success(parsedInstructions);
                return Task.CompletedTask;
            }
            catch
            {
                // If parsing as array fails, try parsing as single object or comma-separated objects
                var jsonDocument = JsonDocument.Parse(normalizedValue);
                var instructions = new List<RequestRecipeInstructionJson>();

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Single object
                    var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(jsonDocument.RootElement.GetRawText());
                    if (instruction != null)
                    {
                        instructions.Add(instruction);
                    }
                }
                else if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                {
                    // Array of objects
                    foreach (var element in jsonDocument.RootElement.EnumerateArray())
                    {
                        var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(element.GetRawText());
                        if (instruction != null)
                        {
                            instructions.Add(instruction);
                        }
                    }
                }

                bindingContext.Result = ModelBindingResult.Success(instructions);
            }
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid JSON format: {ex.Message}");
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}