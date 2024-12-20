using Microsoft.OpenApi.Models;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Tokens;

namespace MyRecipeBook;

public static class ApiDependencyInjectionExtension
{
    private const string AuthenticationType = "Bearer";
    public static void AddApi(this IServiceCollection services)
    {
        AddSwaggerWithTokenReader(services);
        services.AddScoped<ITokenProvider, GetTokenValueFromRequest>();
        services.AddHttpContextAccessor(); //allow context accessor on GetTokenValueFromRequest
    }

    private static void AddSwaggerWithTokenReader(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(AuthenticationType, new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = AuthenticationType
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = AuthenticationType
                        },
                        Scheme = "oauth2",
                        Name = AuthenticationType,
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });
    }
}