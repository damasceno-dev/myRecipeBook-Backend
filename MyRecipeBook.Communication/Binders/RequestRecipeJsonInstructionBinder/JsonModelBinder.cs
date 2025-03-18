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
        var instructions = new List<RequestRecipeInstructionJson>();
        var rawValue = valueProviderResult.FirstValue;
        
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        Console.WriteLine($"Original raw value: {rawValue}");
        
        // CASE 1: Swagger format - JSON array of strings ["string1","string2",...]
        if (rawValue.StartsWith("[\"") && rawValue.EndsWith("\"]"))
        {
            Console.WriteLine("Detected Swagger format - array of strings");
            
            try
            {
                using var jsonDoc = JsonDocument.Parse(rawValue);
                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in jsonDoc.RootElement.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            var jsonString = element.GetString();
                            if (!string.IsNullOrEmpty(jsonString))
                            {
                                Console.WriteLine($"Processing string from array: {jsonString}");
                                
                                try
                                {
                                    // The strings from Swagger already have proper JSON format
                                    // but may contain escape sequences like \n
                                    var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(jsonString);
                                    if (instruction != null)
                                    {
                                        instructions.Add(instruction);
                                        Console.WriteLine($"Successfully parsed instruction: Step {instruction.Step}, Text: {instruction.Text}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to parse instruction from Swagger: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Failed to process Swagger format: {ex.Message}");
            }
        }
        // CASE 2: Frontend format - multiple values or concatenated objects
        else
        {
            Console.WriteLine("Processing frontend format");
            
            // Check if we have multiple values from a form submission
            var allValues = valueProviderResult.Values.ToList();
            if (allValues.Count > 1)
            {
                Console.WriteLine($"Processing {allValues.Count} separate values");
                
                foreach (var value in allValues)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        // Normalize and parse each individual value
                        var normalizedItem = value
                            .Replace("\\n", "")
                            .Replace("\\\"", "\"")
                            .Trim();
                        
                        try
                        {
                            // Try to parse as single object
                            if (normalizedItem.StartsWith("{") && normalizedItem.EndsWith("}"))
                            {
                                var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(normalizedItem);
                                if (instruction != null)
                                {
                                    instructions.Add(instruction);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to parse individual value: {ex.Message}");
                        }
                    }
                }
            }
            
            // If we couldn't parse individual values or there's only one value,
            // try to parse the raw value as a combined JSON structure
            if (instructions.Count == 0 && !string.IsNullOrWhiteSpace(rawValue))
            {
                var normalizedValue = rawValue
                    .Replace("\\n", "")
                    .Replace("\\\"", "\"")
                    .Trim();
                
                Console.WriteLine($"Normalized frontend value: {normalizedValue}");
                
                // CASE 2A: JSON array directly [{"Step":1,...},{"Step":2,...}]
                if (normalizedValue.StartsWith("[") && normalizedValue.EndsWith("]"))
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
                // CASE 2B: Concatenated objects without array notation {"Step":1,...},{"Step":2,...}
                else if (normalizedValue.StartsWith("{") && normalizedValue.Contains("},{") && normalizedValue.EndsWith("}"))
                {
                    Console.WriteLine("Processing as concatenated objects");
                    
                    // Wrap in array brackets and try to parse
                    try
                    {
                        var arrayWrapped = $"[{normalizedValue}]";
                        var parsedInstructions = JsonSerializer.Deserialize<List<RequestRecipeInstructionJson>>(arrayWrapped);
                        if (parsedInstructions != null)
                        {
                            instructions.AddRange(parsedInstructions);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse concatenated objects with wrapping: {ex.Message}");
                        
                        // If that didn't work, try splitting and parsing each part
                        try
                        {
                            string[] parts = normalizedValue.Split(new[] { "},{" }, StringSplitOptions.None);
                            Console.WriteLine($"Split into {parts.Length} parts");
                            
                            for (int i = 0; i < parts.Length; i++)
                            {
                                string part = parts[i];
                                
                                // Add back the braces that were removed in the split
                                if (i == 0)
                                {
                                    if (!part.EndsWith("}"))
                                        part = part + "}";
                                }
                                else if (i == parts.Length - 1)
                                {
                                    if (!part.StartsWith("{"))
                                        part = "{" + part;
                                }
                                else
                                {
                                    part = "{" + part + "}";
                                }
                                
                                try
                                {
                                    var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(part);
                                    if (instruction != null)
                                    {
                                        instructions.Add(instruction);
                                    }
                                }
                                catch (Exception innerEx)
                                {
                                    Console.WriteLine($"Failed to parse part {i}: {innerEx.Message}");
                                }
                            }
                        }
                        catch (Exception splitEx)
                        {
                            Console.WriteLine($"Failed to process by splitting: {splitEx.Message}");
                        }
                    }
                }
                // CASE 2C: Single object {"Step":1,...}
                else if (normalizedValue.StartsWith("{") && normalizedValue.EndsWith("}"))
                {
                    Console.WriteLine("Processing as single object");
                    try
                    {
                        var instruction = JsonSerializer.Deserialize<RequestRecipeInstructionJson>(normalizedValue);
                        if (instruction != null)
                        {
                            instructions.Add(instruction);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to parse as single object: {ex.Message}");
                    }
                }
            }
        }

        Console.WriteLine($"Total instructions parsed: {instructions.Count}");
        bindingContext.Result = ModelBindingResult.Success(instructions);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in model binding: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid JSON format: {ex.Message}");
        bindingContext.Result = ModelBindingResult.Failed();
    }

    return Task.CompletedTask;
}
}