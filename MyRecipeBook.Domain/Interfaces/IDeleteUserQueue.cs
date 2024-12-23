namespace MyRecipeBook.Domain.Interfaces;

public interface IDeleteUserQueue
{
    Task SendMessageAsync(Guid userId);
    Task<IEnumerable<(string MessageBody, string ReceiptHandle)>> ReceiveMessagesAsync(CancellationToken cancellationToken);
    Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken);
}