using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Infrastructure;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace WebApi.Test;

public class MyInMemoryFactory :  WebApplicationFactory<Program>
{
    private readonly HttpClient _httpClient;
    public MyInMemoryFactory()
    {
        _httpClient = CreateClient();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test").ConfigureTestServices(s =>
            { 
                var currentDbContext = s.SingleOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<MyRecipeBookDbContext>));
                if (currentDbContext is not null) s.Remove(currentDbContext);
                s.AddDbContext<MyRecipeBookDbContext>(d => d.UseInMemoryDatabase("TestDatabase"));
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
}