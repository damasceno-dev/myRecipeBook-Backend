using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Application.UseCases.Users.Profile;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Application.UseCases.Users.Update;

namespace MyRecipeBook.Application;

public static class AppDependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(AutoMapping));
        AddUseCases(services);
        AddPasswordEncryption(services, configuration);
    }

    private static void AddPasswordEncryption(IServiceCollection services, IConfiguration configuration)
    {
        var additionalKey = configuration.GetValue<string>("Settings:Password:AdditionalKey");
        if (additionalKey is null)
        {
            throw new ArgumentException("Invalid additional key for password encryption");
        }
        services.AddScoped(options => new PasswordEncryption(additionalKey));
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<UserRegisterUseCase>();
        services.AddScoped<UserLoginUseCase>();        
        services.AddScoped<UserProfileWithTokenUseCase>();
        services.AddScoped<UserUpdateUseCase>();
    }
}