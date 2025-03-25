using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;

public class JsonModelBinder : IModelBinder
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

            Console.WriteLine($"Original raw value: {rawValue}");
            
            var instructions = ParseInstructions(rawValue, valueProviderResult.Values);
            
            Console.WriteLine($"Total instructions parsed: {instructions.Count}");
            bindingContext.Result = ModelBindingResult.Success(instructions);
        }
        catch (Exception ex)
        {
            LogBindingError(ex, bindingContext);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

    private List<RequestRecipeInstructionJson> ParseInstructions(string rawValue, StringValues values)
    {
        var instructions = new List<RequestRecipeInstructionJson>();

        // Try to parse as Swagger format first
        if (IsSwaggerFormat(rawValue))
        {
            ParseSwaggerFormat(rawValue, instructions);
        }
        else
        {
            // Handle frontend format
            ParseFrontendFormat(rawValue, values, instructions);
        }

        return instructions;
    }

    private static bool IsSwaggerFormat(string value)
    {
        return value.StartsWith("[\"") && value.EndsWith("\"]");
    }

    private static void ParseSwaggerFormat(string rawValue, List<RequestRecipeInstructionJson> instructions)
    {
        Console.WriteLine("Detected Swagger format - array of strings");
        
        try
        {
            using var jsonDoc = JsonDocument.Parse(rawValue);
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                ProcessJsonArray(jsonDoc.RootElement, instructions);
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Failed to process Swagger format: {ex.Message}");
        }
    }

    private static void ProcessJsonArray(JsonElement arrayElement, List<RequestRecipeInstructionJson> instructions)
    {
        foreach (var element in arrayElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String) continue;

            var jsonString = element.GetString();
            if (string.IsNullOrEmpty(jsonString)) continue;

            Console.WriteLine($"Processing string from array: {jsonString}");
            TryParseAndAddInstruction(jsonString, instructions, "instruction from Swagger");
        }
    }

    private void ParseFrontendFormat(string rawValue, StringValues values, List<RequestRecipeInstructionJson> instructions)
    {
        Console.WriteLine("Processing frontend format");
        
        // Check if we have multiple values from a form submission
        var allValues = values.ToList();
        if (allValues.Count > 1)
        {
            ProcessMultipleValues(allValues, instructions);
        }
        
        // If we couldn't parse individual values or there's only one value
        if (instructions.Count == 0 && !string.IsNullOrWhiteSpace(rawValue))
        {
            ProcessSingleRawValue(rawValue, instructions);
        }
    }

    private static void ProcessMultipleValues(List<string?> values, List<RequestRecipeInstructionJson> instructions)
    {
        Console.WriteLine($"Processing {values.Count} separate values");
        
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            
            var normalizedItem = NormalizeJsonString(value);
            
            if (IsSingleJsonObject(normalizedItem))
            {
                TryParseAndAddInstruction(normalizedItem, instructions, "individual value");
            }
        }
    }

    private void ProcessSingleRawValue(string rawValue, List<RequestRecipeInstructionJson> instructions)
    {
        var normalizedValue = NormalizeJsonString(rawValue);
        
        Console.WriteLine($"Normalized frontend value: {normalizedValue}");
        
        if (IsJsonArray(normalizedValue))
        {
            ProcessJsonArrayString(normalizedValue, instructions);
        }
        else if (IsConcatenatedObjects(normalizedValue))
        {
            ProcessConcatenatedObjects(normalizedValue, instructions);
        }
        else if (IsSingleJsonObject(normalizedValue))
        {
            TryParseAndAddInstruction(normalizedValue, instructions, "single object");
        }
    }

    private static void ProcessJsonArrayString(string normalizedValue, List<RequestRecipeInstructionJson> instructions)
    {
        Console.WriteLine("Processing as JSON array");
        try
        {
            var parsedInstructions = JsonSerializer.Deserialize<List<RequestRecipeInstructionJson>>(normalizedValue);
            if (parsedInstructions != null)
            {
                instructions.AddRange(parsedInstructions);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse as JSON array: {ex.Message}");
        }
    }

    private static void ProcessConcatenatedObjects(string normalizedValue, List<RequestRecipeInstructionJson> instructions)
    {
        Console.WriteLine("Processing as concatenated objects");
        
        // Try with array wrapping first
        if (TryParseWithArrayWrapping(normalizedValue, instructions))
        {
            return;
        }
        
        // Fall back to manual splitting
        TrySplitAndParseParts(normalizedValue, instructions);
    }

    private static bool TryParseWithArrayWrapping(string normalizedValue, List<RequestRecipeInstructionJson> instructions)
    {
        try
        {
            var arrayWrapped = $"[{normalizedValue}]";
            var parsedInstructions = JsonSerializer.Deserialize<List<RequestRecipeInstructionJson>>(arrayWrapped);
            if (parsedInstructions != null)
            {
                instructions.AddRange(parsedInstructions);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse concatenated objects with wrapping: {ex.Message}");
        }
        
        return false;
    }

    private static void TrySplitAndParseParts(string normalizedValue, List<RequestRecipeInstructionJson> instructions)
    {
        try
        {
            string[] parts = normalizedValue.Split(["},{"], StringSplitOptions.None);
            Console.WriteLine($"Split into {parts.Length} parts");
            
            for (int i = 0; i < parts.Length; i++)
            {
                string part = FixBracesForPart(parts[i], i, parts.Length);
                TryParseAndAddInstruction(part, instructions, $"part {i}");
            }
        }
        catch (Exception splitEx)
        {
            Console.WriteLine($"Failed to process by splitting: {splitEx.Message}");
        }
    }

    private static string FixBracesForPart(string part, int index, int totalParts)
    {
        if (index == 0 && !part.EndsWith('}'))
        {
            return part + "}";
        }

        if (index == totalParts - 1 && !part.StartsWith('{'))
        {
            return "{" + part;
        }

        if (index > 0 && index < totalParts - 1)
        {
            return "{" + part + "}";
        }

        return part;
    }

    private static void TryParseAndAddInstruction(string json, List<RequestRecipeInstructionJson> instructions, string context)
    {
        try
        {
            var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(json);
            if (instruction != null)
            {
                instructions.Add(instruction);
                if (instruction.Step > 0)
                {
                    Console.WriteLine($"Successfully parsed instruction: Step {instruction.Step}, Text: {instruction.Text}");
                }
                else
                {
                    Console.WriteLine($"Warning: Parsed instruction with Step <= 0: {instruction.Step}, Text: {instruction.Text}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse {context}: {ex.Message}");
        }
    }

    private static string NormalizeJsonString(string value)
    {
        return value
            .Replace("\\n", "")
            .Replace("\\\"", "\"")
            .Trim();
    }

    private static bool IsJsonArray(string text)
    {
        return text.StartsWith('[') && text.EndsWith(']');
    }

    private static bool IsConcatenatedObjects(string text)
    {
        return text.StartsWith('{') && text.Contains("},{") && text.EndsWith('}');
    }

    private static bool IsSingleJsonObject(string text)
    {
        return text.StartsWith('{') && text.EndsWith('}');
    }

    private static void LogBindingError(Exception ex, ModelBindingContext bindingContext)
    {
        Console.WriteLine($"Exception in model binding: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid JSON format: {ex.Message}");
    }
}