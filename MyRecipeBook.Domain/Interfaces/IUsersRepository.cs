using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IUsersRepository
{
    void Register(User newUser);
}