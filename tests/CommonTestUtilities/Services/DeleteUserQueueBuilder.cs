using Moq;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Services;
public class DeleteUserQueueBuilder
{
    public static IDeleteUserQueue Build()
    {
        return new Mock<IDeleteUserQueue>().Object;
    }
}