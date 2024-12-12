using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyRecipeBook.Filters;

/// <summary>
/// Indicates in swagger that the Instructions should be placed as json, in the format [ {}, {}, ... ]
/// Removes Instruction query parameter from Parameters. Needed because the Instruction property in RequestRecipeForm is binded, and swagger interprets it as a query parameter
/// </summary>
public class SwaggerRequestRecipeFormInstructionsFilter : IOperationFilter
{
    private const string MultipartFormData = "multipart/form-data";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {        
        foreach (var parameter in context.ApiDescription.ParameterDescriptions)
        {
            var parameterType = parameter.ParameterDescriptor?.ParameterType;

            if (parameterType is { Name: "RequestRecipeForm" })
            {
                //Remove Instruction query parameter
               operation.Parameters = operation.Parameters
                 .Where(p => p.Name != "Instructions")
                .ToList();
                HandleInstructionsProperty(operation);
                break; // Since we're only handling `Instructions`, exit loop after processing
            }
        }
    }
    private static void HandleInstructionsProperty(OpenApiOperation operation)
    {
        if (operation.RequestBody?.Content == null) return;

        if (!operation.RequestBody.Content.TryGetValue(MultipartFormData, out var mediaType))
        {
            mediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>()
                },
                Encoding = new Dictionary<string, OpenApiEncoding>()
            };
            operation.RequestBody.Content[MultipartFormData] = mediaType;
        }

        // Define the schema for the Instructions property as an array
        mediaType.Schema.Properties["Instructions"] = new OpenApiSchema
        {
            Type = "json",
            Items = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "Step", new OpenApiSchema { Type = "integer", Format = "int32" } },
                    { "Text", new OpenApiSchema { Type = "string" } }
                }
            },
            Example = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["Step"] = new OpenApiInteger(1),
                    ["Text"] = new OpenApiString("step 1")
                },
                new OpenApiObject
                {
                    ["Step"] = new OpenApiInteger(2),
                    ["Text"] = new OpenApiString("step 2")
                },
                
            }
        };
    }


}
