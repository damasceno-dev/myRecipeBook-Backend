using Amazon.SQS;
using Amazon.SQS.Model;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Services;

public class AwsSimpleQueueService(IAmazonSQS sqsClient, string queueUrl) : IDeleteUserQueue
{
    public async Task SendMessageAsync(Guid userId)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = userId.ToString()
        };

        await sqsClient.SendMessageAsync(request);
    }
    
    public async Task<IEnumerable<(string MessageBody, string ReceiptHandle)>> ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 10
        }, cancellationToken);

        return response.Messages.Select(message => (message.Body, message.ReceiptHandle));
    }

    public async Task DeleteMessageAsync(string receiptHandle, CancellationToken cancellationToken)
    {
        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = queueUrl,
            ReceiptHandle = receiptHandle
        }, cancellationToken);
    }
}