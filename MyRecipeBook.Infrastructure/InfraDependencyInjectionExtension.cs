using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyRecipeBook.Infrastructure;

public static class InfraDependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
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