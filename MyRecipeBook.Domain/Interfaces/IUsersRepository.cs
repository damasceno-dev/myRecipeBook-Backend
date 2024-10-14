using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IUsersRepository
{
    Task Register(User newUser);
    Task<bool> ExistsActiveUserWithEmail(string email);
    Task<User?> GetExistingUserWithEmail(string email);
}