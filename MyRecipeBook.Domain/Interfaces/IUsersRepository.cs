using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IUsersRepository
{
    Task Register(User newUser);
}