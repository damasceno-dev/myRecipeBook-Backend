using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Infrastructure;
using Testcontainers.PostgreSql;
using Xunit;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace WebApi.Test;

public class MyContainerFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    public MyRecipeBookDbContext RecipeDbContext { get; set; } = default!;
    public MyContainerFactory()
    {
        _httpClient = CreateClient();
    }
    
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
    public async Task<HttpResponseMessage> DoPost<T>(string route, T request, string? culture = null)
    {
        if (culture is not null)
        {
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        }
        return await _httpClient.PostAsJsonAsync(route, request);
    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();
        
        var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<MyRecipeBookDbContext>().Database.MigrateAsync();
        RecipeDbContext = scope.ServiceProvider.GetRequiredService<MyRecipeBookDbContext>();
    }

    public new async Task DisposeAsync()
    {
        await _databaseContainer.StopAsync();
    }
}