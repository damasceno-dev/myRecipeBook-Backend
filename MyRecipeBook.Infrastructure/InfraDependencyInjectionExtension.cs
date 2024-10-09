using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Infrastructure.Repositories;

namespace MyRecipeBook.Infrastructure;

public static class InfraDependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var testEnv = configuration.GetValue<bool>("IsTestEnvironment");
        AddRepositories(services);
        if (testEnv is false)
        {
            AddDbContext(services, configuration);
        }
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        DotNetEnv.Env.Load("../MyRecipeBook.Infrastructure/.env");
        var envPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        
        if (envPassword is null)
            throw new Exception("Invalid password from .env");
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (connectionString is null)
            throw new Exception("Invalid connection string");
        
        connectionString = connectionString.Replace("$$password$$", envPassword);

        services.AddDbContext<MyRecipeBookDbContext>(options =>{options.UseNpgsql(connectionString);});
    }
}