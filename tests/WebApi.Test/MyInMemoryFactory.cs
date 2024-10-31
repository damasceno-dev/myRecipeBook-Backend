using System.Net.Http.Headers;
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
        AddCulture(culture);
        return await _httpClient.PostAsJsonAsync(route, request);
    }

    public async Task<HttpResponseMessage> DoGet(string route, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        return await _httpClient.GetAsync(route);
    }
    
    public async Task<HttpResponseMessage> DoPut<T>(string route, T request, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        return await _httpClient.PutAsJsonAsync(route, request);
    }
    
    private void AddToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null; //clear between requests
        if (string.IsNullOrWhiteSpace(token) is false)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private void AddCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture) is false)
        {
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        }
    }

}