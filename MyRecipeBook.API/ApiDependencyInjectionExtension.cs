using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Tokens;

namespace MyRecipeBook;

public static class ApiDependencyInjectionExtension
{
    public static void AddApi(this IServiceCollection services)
    {
        services.AddScoped<ITokenProvider, GetTokenValueFromRequest>();
        services.AddHttpContextAccessor();
    }
}