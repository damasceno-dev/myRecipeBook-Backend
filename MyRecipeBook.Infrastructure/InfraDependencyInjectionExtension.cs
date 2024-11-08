using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Infrastructure.Repositories;
using MyRecipeBook.Infrastructure.Tokens;

namespace MyRecipeBook.Infrastructure;

public static class InfraDependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var testEnv = configuration.GetValue<bool>("IsTestEnvironment");
        AddRepositories(services);
        AddToken(services, configuration);
        if (testEnv is false)
        {
            AddDbContext(services, configuration);
        }
    }

    private static void AddToken(IServiceCollection services, IConfiguration configuration)
    {
        var signKey = configuration.GetValue<string>("Settings:Token:SignKey");
        var expirationTime = configuration.GetValue<int>("Settings:Token:ExpirationTimeInMinutes");
        if (signKey is null || expirationTime == 0)
            throw new ArgumentException("Invalid token sign key or expiration time");
        services.AddScoped<ITokenRepository>(options => new JsonWebTokenRepository(expirationTime, signKey));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRecipesRepository, RecipesRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        DotNetEnv.Env.Load("../MyRecipeBook.Infrastructure/.env");
        var envPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        
        if (envPassword is null)
            throw new ArgumentException("Invalid password from .env");
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (connectionString is null)
            throw new ArgumentException("Invalid connection string");
        
        connectionString = connectionString.Replace("$$password$$", envPassword);

        services.AddDbContext<MyRecipeBookDbContext>(options =>{options.UseNpgsql(connectionString);});
    }
}