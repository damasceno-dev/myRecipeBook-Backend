using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Application.AutoMapper;
using MyRecipeBook.Application.UseCases.Users.Register;

namespace MyRecipeBook.Application;

public static class AppDependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapping));
        services.AddScoped<UserRegisterUseCase>();
    } 
}