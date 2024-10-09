using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Infrastructure;

namespace WebApi.Test;

public class MyInMemoryFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test").ConfigureTestServices(s =>
            { 
                var currentDbContext = s.SingleOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<MyRecipeBookDbContext>));
                if (currentDbContext is not null) s.Remove(currentDbContext);
                s.AddDbContext<MyRecipeBookDbContext>(d => d.UseInMemoryDatabase("TestDatabase"));
            });
    }
}