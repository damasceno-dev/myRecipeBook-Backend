using MyRecipeBook.Application.UseCases.Users.Delete;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.BackgroundServices;

public class AwsQueueDeleteUserBackgroundService(IServiceProvider services, IDeleteUserQueue deleteUserQueue)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await deleteUserQueue.ReceiveMessagesAsync(stoppingToken);

            foreach (var (messageBody, receiptHandle) in messages)
            {
                await ProcessMessageAsync(messageBody);
                await deleteUserQueue.DeleteMessageAsync(receiptHandle, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(string messageBody)
    {
        var userIdentifier = Guid.Parse(messageBody);

        using var scope = services.CreateScope();
        var deleteUserUseCase = scope.ServiceProvider.GetRequiredService<UserDeleteUseCase>();

        await deleteUserUseCase.Execute(userIdentifier);
    }

    ~AwsQueueDeleteUserBackgroundService() => Dispose();
    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}