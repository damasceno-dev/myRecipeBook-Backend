using Amazon.S3;
using Amazon.S3.Model;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Services;

public class AwsStorageService(IAmazonS3 s3Client, string bucketName): IStorageService
{
    private const int MaximumImageUrlLifetimeInMinutes = 10;

    public async Task Upload(User user, Stream file, string fileName)
    {
        var key = $"{user.Id}/{fileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = file,
            AutoCloseStream = true
        };

        await s3Client.PutObjectAsync(putRequest);
    }

    public async Task<string> GetFileUrl(User user, string fileName)
    {
        var key = $"{user.Id}/{fileName}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(MaximumImageUrlLifetimeInMinutes),
            Verb = HttpVerb.GET
        };

        return await s3Client.GetPreSignedURLAsync(request);
    }

    public async Task Delete(User user, string fileName)
    {
        var key = $"{user.Id}/{fileName}";

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        await s3Client.DeleteObjectAsync(deleteRequest);
    }

    public async Task DeleteContainer(Guid userIdentifier)
    {
        var listRequest = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = userIdentifier.ToString()
        };

        var listResponse = await s3Client.ListObjectsV2Async(listRequest);

        foreach (var obj in listResponse.S3Objects)
        {
            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = obj.Key
            });
        }
    }
}