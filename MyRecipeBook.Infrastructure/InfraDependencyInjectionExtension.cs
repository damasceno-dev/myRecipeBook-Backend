using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Email;
using MyRecipeBook.Domain.Interfaces.OpenAI;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Infrastructure.Repositories;
using MyRecipeBook.Infrastructure.Services;
using MyRecipeBook.Infrastructure.Tokens;
using OpenAI.Chat;

namespace MyRecipeBook.Infrastructure;

public static class InfraDependencyInjectionExtension
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        try
        {
            var dbContext = scopedServices.GetRequiredService<MyRecipeBookDbContext>();
            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (!pendingMigrations.Any())
            {
                Console.WriteLine(@"No pending migrations were found. The database is up-to-date.");
                return;
            }

            Console.WriteLine(@"The following migrations will be applied:");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($@"- {migration}");
            }

            await dbContext.Database.MigrateAsync();
            Console.WriteLine(@"Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"An error occurred during database migration: {ex.Message}");
            throw;
        }
    }
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDotEnvFiles();
        var testEnv = configuration.GetValue<bool>("IsTestEnvironment");
        AddRepositories(services);
        AddToken(services, configuration);
        if (testEnv is false)
        {
            AddDbContext(services, configuration);
            AddOpenAI(services);
            AddAwsStorage(services);
            AddAwsQueue(services);
            AddEmailSender(services);
        }

    }

    private static void AddDotEnvFiles()
    {    
        // Determine the environment and set the .env file path
        var envFilePath = File.Exists("Infrastructure.env")
            ? "Infrastructure.env" // Path for publishing environment
            : "../MyRecipeBook.Infrastructure/Infrastructure.env"; // Path for development environment

        DotNetEnv.Env.Load(envFilePath);
    }
    
    private static void AddEmailSender(IServiceCollection services)
    {
        services.AddScoped<ISendUserResetPasswordCode, EmailUserResetPasswordCode>();
    }

    private static void AddToken(IServiceCollection services, IConfiguration configuration)
    {
        var signKey = configuration.GetValue<string>("Settings:Token:SignKey");
        var expirationTime = configuration.GetValue<int>("Settings:Token:ExpirationTimeInMinutes");
        if (signKey is null || expirationTime == 0)
            throw new ArgumentException("Invalid token sign key or expiration time");
        services.AddScoped<ITokenRepository>(_ => new JsonWebTokenRepository(expirationTime, signKey));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRecipesRepository, RecipesRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
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
        
        var bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");

        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentException("Invalid or empty AWS S3 Bucket Name");

        var region = Environment.GetEnvironmentVariable("AWS_REGION");
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) ||string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("Invalid AWS connection string values");
        
        var s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        services.AddScoped<IStorageService>(_ => new AwsStorageService(s3Client, bucketName));
    }
    
    private static void AddAwsQueue(IServiceCollection services)
    {
        var queueUrl = Environment.GetEnvironmentVariable("AWS_SQS_DELETE_USER_URL");
        
        if (string.IsNullOrWhiteSpace(queueUrl))
            throw new ArgumentException("Invalid AWS SQS Delete user url");
       
        var region = Environment.GetEnvironmentVariable("AWS_REGION");
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) ||string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("Invalid AWS connection string values");
        
        services.AddSingleton<IDeleteUserQueue>(_ => new AwsSimpleQueueService(new AmazonSQSClient(), queueUrl));
    }
}