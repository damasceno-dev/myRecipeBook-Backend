using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyRecipeBook.Filters;

/// <summary>
/// Configures the Instructions property to be displayed as a JSON array in Swagger.
/// Requires the nested object to be binded with the JsonModelBinder
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
                // Remove incorrectly added query parameter for Instructions
                operation.Parameters = operation.Parameters
                    .Where(p => p.Name != "Instructions")
                    .ToList();

                ConfigureInstructionsProperty(operation);
                break;
            }
        }
    }

    private static void ConfigureInstructionsProperty(OpenApiOperation operation)
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

            // Define the Instructions property
            mediaType.Schema.Properties["Instructions"] = new OpenApiSchema
            {
                Type = "array",
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
                        ["Text"] = new OpenApiString("First step description")
                    },
                    new OpenApiObject
                    {
                        ["Step"] = new OpenApiInteger(2),
                        ["Text"] = new OpenApiString("Second step description")
                    }
                }
            };

            // Configure `Instructions` with application/json content type
            mediaType.Encoding["Instructions"] = new OpenApiEncoding
            {
                ContentType = "application/json"
            };
        
    }
}
