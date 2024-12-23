using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.OpenAI;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Infrastructure.Repositories;
using MyRecipeBook.Infrastructure.Services;
using MyRecipeBook.Infrastructure.Tokens;
using OpenAI.Chat;

namespace MyRecipeBook.Infrastructure;

public static class InfraDependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        DotNetEnv.Env.Load("../MyRecipeBook.Infrastructure/.env");
        var testEnv = configuration.GetValue<bool>("IsTestEnvironment");
        AddRepositories(services);
        AddToken(services, configuration);
        if (testEnv is false)
        {
            AddDbContext(services, configuration);
            AddOpenAI(services);
            AddAwsStorage(services);
            AddAwsQueue(services);
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
        var envPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        
        if (envPassword is null)
            throw new ArgumentException("Invalid password from .env");
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (connectionString is null)
            throw new ArgumentException("Invalid connection string");
        
        connectionString = connectionString.Replace("$$password$$", envPassword);

        services.AddDbContext<MyRecipeBookDbContext>(options => options.UseNpgsql(connectionString));
    }
    
    private static void AddOpenAI(IServiceCollection services)
    {
        services.AddScoped<IRecipeAIGenerator, ChatGptService>();
        
        var openAIKey = Environment.GetEnvironmentVariable("OPEN_API_KEY");
        services.AddScoped(_ => new ChatClient(ChatGptService.ChatModel, openAIKey));
    }
    
    private static void AddAwsStorage(IServiceCollection services)
    {
        var awsConfig = Environment.GetEnvironmentVariable("AWS_S3_CONFIG");

        if (string.IsNullOrWhiteSpace(awsConfig))
            throw new ArgumentException("Invalid AWS connection string");
        
        var configParts = awsConfig.Split(';')
            .Select(part => part.Split('='))
            .ToDictionary(kv => kv[0], kv => kv[1]);

        var accessKey = configParts.GetValueOrDefault("AccessKey");
        var secretKey = configParts.GetValueOrDefault("SecretKey");
        var bucketName = configParts.GetValueOrDefault("BucketName");
        var region = configParts.GetValueOrDefault("Region");

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) ||string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentException("Invalid AWS connection string values");
        
        var s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        services.AddScoped<IStorageService>(c => new AwsStorageService(s3Client, bucketName));
    }
    private static void AddAwsQueue(IServiceCollection services)
    {
        var queueUrl = Environment.GetEnvironmentVariable("AWS_SQS_DELETE_USER_URL");

        if (string.IsNullOrWhiteSpace(queueUrl))
            throw new ArgumentException("Invalid AWS SQS Delete user url");
        
        services.AddSingleton<IDeleteUserQueue>(_ => new AwsSimpleQueueService(new AmazonSQSClient(), queueUrl));
    }
}