using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Infrastructure;
using Testcontainers.PostgreSql;
using Xunit;

namespace WebApi.Test;

public class MyContainerFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
            .WithUsername("postgres")
            .WithPassword("testPassword")
            .WithDatabase("myRecipeBook")
            .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test").ConfigureTestServices(s =>
        {
            var currentDbContext = s.SingleOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<MyRecipeBookDbContext>));
            if (currentDbContext is not null) s.Remove(currentDbContext);
            s.AddDbContext<MyRecipeBookDbContext>(options => options.UseNpgsql(_databaseContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();
        await Services.CreateScope().ServiceProvider.GetRequiredService<MyRecipeBookDbContext>().Database.MigrateAsync();

    }

    public new async Task DisposeAsync()
    {
        await _databaseContainer.StopAsync();
    }
}