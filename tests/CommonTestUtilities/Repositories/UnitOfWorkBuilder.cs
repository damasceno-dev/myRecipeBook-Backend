using Moq;
using MyRecipeBook.Domain.Interfaces;

namespace CommonTestUtilities.Repositories;

public static class UnitOfWorkBuilder
{
    public static IUnitOfWork Build()
    {
        return new Mock<IUnitOfWork>().Object;
    }
}