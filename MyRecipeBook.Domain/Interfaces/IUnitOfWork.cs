namespace MyRecipeBook.Domain.Interfaces;

public interface IUnitOfWork
{
    Task Commit();
}