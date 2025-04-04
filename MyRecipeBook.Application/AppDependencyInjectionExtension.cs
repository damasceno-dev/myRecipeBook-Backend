using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Recipes.DeleteById;
using MyRecipeBook.Application.UseCases.Recipes.Filter;
using MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;
using MyRecipeBook.Application.UseCases.Recipes.GetById;
using MyRecipeBook.Application.UseCases.Recipes.GetByUser;
using MyRecipeBook.Application.UseCases.Recipes.ImageUpdateCover;
using MyRecipeBook.Application.UseCases.Recipes.Register;
using MyRecipeBook.Application.UseCases.Recipes.Update;
using MyRecipeBook.Application.UseCases.Users.ChangePassword;
using MyRecipeBook.Application.UseCases.Users.Delete;
using MyRecipeBook.Application.UseCases.Users.ExternalLogin;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Application.UseCases.Users.Profile;
using MyRecipeBook.Application.UseCases.Users.RefreshTokens;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Application.UseCases.Users.ResetPassword;
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
        var defaultExternalLoginKey = configuration.GetValue<string>("Settings:Password:DefaultExternalLoginKey");
        if (additionalKey is null || defaultExternalLoginKey is null)
        {
            throw new ArgumentException("Invalid additional key or external login key for password encryption");
        }
        services.AddScoped(options => new PasswordEncryption(additionalKey,defaultExternalLoginKey));
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<UserRegisterUseCase>();
        services.AddScoped<UserLoginUseCase>();        
        services.AddScoped<UserProfileWithTokenUseCase>();
        services.AddScoped<UserUpdateUseCase>();
        services.AddScoped<UserChangePasswordUseCase>();
        services.AddScoped<RecipeRegisterUseCase>();
        services.AddScoped<RecipeFilterUseCase>();
        services.AddScoped<RecipeGetByIdUseCase>();
        services.AddScoped<RecipeDeleteByIdUseCase>();
        services.AddScoped<RecipeUpdateUseCase>();
        services.AddScoped<RecipeGetByUserUseCase>();
        services.AddScoped<RecipeGenerateWithAIUseCase>();
        services.AddScoped<RecipeUpdateImageUseCase>();
        services.AddScoped<UserDeleteUseCase>();
        services.AddScoped<UserQueueDeletionUseCase>();
        services.AddScoped<UserExternalLoginUseCase>();
        services.AddScoped<UserGetResetPasswordCodeUseCase>();
        services.AddScoped<UserResetPasswordUseCase>();
        services.AddScoped<UserRefreshTokenUseCase>();
    }
}