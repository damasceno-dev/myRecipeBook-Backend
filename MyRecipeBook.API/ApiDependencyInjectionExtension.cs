using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using MyRecipeBook.BackgroundServices;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Filters;
using MyRecipeBook.Tokens;

namespace MyRecipeBook;

public static class ApiDependencyInjectionExtension
{
    private const string AuthenticationType = "Bearer";
    public static void AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        AddDotEnvFiles();
        AddSwaggerWithTokenReaderAndOperationFilter(services);
        services.AddScoped<ITokenProvider, GetTokenValueFromRequest>();
        services.AddHttpContextAccessor(); //allow context accessor on GetTokenValueFromRequest
        var testEnv = configuration.GetValue<bool>("IsTestEnvironment");
        if (testEnv is false)
        {
            services.AddHostedService<AwsQueueDeleteUserBackgroundService>();
            AddGoogleAuthentication(services);
        } 
        AddFrontEndCors(services, "3000");
    }
    private static void AddDotEnvFiles()
    {    
        // Determine the environment and set the .env file path'
        var envFilePath = File.Exists("API.env")
            ? "API.env" // Path for publishing environment
            : "../MyRecipeBook.API/API.env"; // Path for development environment

        DotNetEnv.Env.Load(envFilePath);
    }

    private static void AddFrontEndCors(IServiceCollection services, string port)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins($"http://localhost:{port}") // Frontend URL
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Required for cookies
            });
        });
    }

    private static void AddGoogleAuthentication(IServiceCollection services)
    {
        var googleLoginConfig = Environment.GetEnvironmentVariable("GOOGLE_LOGIN");
        if (string.IsNullOrWhiteSpace(googleLoginConfig))
            throw new ArgumentException("Invalid Google Login connection string");
        
        var configParts = googleLoginConfig.Split(';')
            .Select(part => part.Split('='))
            .ToDictionary(kv => kv[0], kv => kv[1]);

        var clientId = configParts.GetValueOrDefault("ClientId");
        var clientSecret = configParts.GetValueOrDefault("ClientSecret");
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Invalid Google Login connection string values");
        
        services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;

            });
    }

    private static void AddSwaggerWithTokenReaderAndOperationFilter(IServiceCollection services)
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

            options.OperationFilter<SwaggerRequestRecipeFormInstructionsFilter>();
        });
    }
}