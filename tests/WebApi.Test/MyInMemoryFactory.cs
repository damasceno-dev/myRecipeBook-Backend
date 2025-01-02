using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Infrastructure;
using WebApi.Test.Users.LoginExternal;
using Xunit;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace WebApi.Test;

public class MyInMemoryFactory :  WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private User _user = default!;
    private List<Recipe> _recipes = [];
    private string _password = string.Empty;
    
    private MyRecipeBookDbContext? _dbContext;
    private UserPasswordResetCode _userResetPasswordCode = default!;

    public MyRecipeBookDbContext GetDbContext()
    {
        if (_dbContext == null)
        {
            throw new InvalidOperationException("DbContext has not been initialized.");
        }

        return _dbContext;
    }
    public User GetUser() => _user;
    public List<Recipe> GetRecipes() => _recipes;
    public string GetPassword() => _password;
    public UserPasswordResetCode GetResetPasswordCode() => _userResetPasswordCode;

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
            AddDatabase(s);
            AddOpenAIMock(s);
            AddAwsStorageMock(s);
            AddGoogleLoginAuthenticationMock(s);
            AddEmailServicesMock(s);
        });
    }

    private static void AddEmailServicesMock(IServiceCollection service)
    {
        var sendEmailMock = SendUserResetPasswordCodeBuilder.Build();
        service.AddScoped(_ => sendEmailMock);
    }

    private static void AddGoogleLoginAuthenticationMock(IServiceCollection service)
    {    
        var mockAuthenticationService = new Mock<IAuthenticationService>();
        mockAuthenticationService
            .Setup(auth => auth.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, LoginGoogleUserControllerInMemoryTest.GoogleEmail),
                    new Claim(ClaimTypes.Name, LoginGoogleUserControllerInMemoryTest.GoogleName)
                }, "mock")),
                "mock")));

        service.AddSingleton(mockAuthenticationService.Object);
    }

    private static void AddAwsStorageMock(IServiceCollection service)
    {
        var awsStorageMock = new StorageServiceBuilder().Upload().Delete().GetFileUrl().Build();
        service.AddScoped(_ => awsStorageMock);
    }

    private static void AddOpenAIMock(IServiceCollection service)
    {
        var chatGptMockRecipeGenerator = new ChatGptServiceBuilder().GenerateAIRecipe(RecipeDtoBuilder.Build()).Build();
        service.AddScoped(_ => chatGptMockRecipeGenerator);
    }

    private void AddDatabase(IServiceCollection service)
    {
        service.AddDbContext<MyRecipeBookDbContext>(d => d.UseInMemoryDatabase("TestDatabase"));
        _dbContext = service.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<MyRecipeBookDbContext>();
    }

    public async Task InitializeAsync()
    {
        if (_dbContext == null)
        {
            throw new InvalidOperationException("DbContext has not been initialized.");
        }
        
        (_user, _password) = UserBuilder.Build();
        _recipes = RecipeBuilder.RecipeCollection(_user);
        _userResetPasswordCode = UserPasswordCodeBuilder.Build(_user.Id, DigitGenerator.Generate6DigitCode());
        
        _dbContext.Users.Add(_user);
        _dbContext.Recipes.AddRange(_recipes);
        _dbContext.UserPasswordResetCodes.Add(_userResetPasswordCode);
        
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<HttpResponseMessage> DoPost<T>(string route, T request, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        return await _httpClient.PostAsJsonAsync(route, request);
    }
    
    public async Task<HttpResponseMessage> DoPostRecipeForm(string route, RequestRecipeForm request, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
    
        using var content = new MultipartFormDataContent();
        
        if (request.ImageFile != null)
        {
            var streamContent = new StreamContent(request.ImageFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.ImageFile.ContentType);
            content.Add(streamContent, nameof(request.ImageFile), request.ImageFile.FileName);
        }
    
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            content.Add(new StringContent(request.Title), nameof(request.Title));
        }
    
        if (request.CookingTime != null)
        {
            var cookingTimeValue = Convert.ToInt32(request.CookingTime);
            content.Add(new StringContent(cookingTimeValue.ToString()), nameof(request.CookingTime));
        }
    
        if (request.Difficulty != null)
        {
            var difficultyValue = Convert.ToInt32(request.Difficulty);
            content.Add(new StringContent(difficultyValue.ToString()), nameof(request.Difficulty));
        }
    
        foreach (var dishType in request.DishTypes)
        {
            var dishTypeValue = Convert.ToInt32(dishType); // Keeps it simple for invalid enums
            content.Add(new StringContent(dishTypeValue.ToString()), $"{nameof(request.DishTypes)}[]");
        }
    
        foreach (var ingredient in request.Ingredients)
        {
            content.Add(new StringContent(ingredient), $"{nameof(request.Ingredients)}[]");
        }
    
        if (request.Instructions.Any())
        {
            var instructionsJson = JsonSerializer.Serialize(request.Instructions);
            content.Add(new StringContent(instructionsJson, Encoding.UTF8, "application/json"), nameof(request.Instructions));
        }
        
        return await _httpClient.PostAsync(route, content);
    }


    public async Task<HttpResponseMessage> DoGet(string route, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        return await _httpClient.GetAsync(route);
    }
    
    public async Task<HttpResponseMessage> DoDelete(string route, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        return await _httpClient.DeleteAsync(route);
    }
    
    public async Task<HttpResponseMessage> DoPut<T>(string route, T request, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        var response = await _httpClient.PutAsJsonAsync(route, request);
        return response;
    }
    public async Task<HttpResponseMessage> DoPutMultipartForm(string route, MultipartFormDataContent content, string? culture = null, string? token = null)
    {
        AddCulture(culture);
        AddToken(token);
        var response =  await _httpClient.PutAsync(route, content);
        return response;
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
    
    public new async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}