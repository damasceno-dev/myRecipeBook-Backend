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
                policy.WithOrigins($"http://localhost:{port}", $"https://localhost:{port}", "https://main.d39u9fqzs1y4j1.amplifyapp.com") // Frontend URL
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
                // Use less strict cookie settings for development over HTTP:
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                options.Events.OnRedirectToAuthorizationEndpoint = context =>
                {
                    var redirectUri = context.RedirectUri;
                    var uri = new Uri(redirectUri);
                    // Parse the query parameters
                    var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    // Convert to a dictionary to override values
                    var queryDict = queryParams.ToDictionary(k => k.Key, v => v.Value.ToString());
                    // Override the prompt parameter unconditionally
                    queryDict["prompt"] = "consent select_account";
                    // Rebuild the redirect URI
                    var newRedirectUri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), queryDict!);
                    context.Response.Redirect(newRedirectUri);
                    return Task.CompletedTask;
                };
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