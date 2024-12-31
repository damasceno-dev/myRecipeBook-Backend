using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IUsersRepository
{
    Task Register(User newUser);
    Task<bool> ExistsActiveUserWithEmail(string email);
    Task<User?> GetExistingUserWithEmail(string email);
    Task<User?> GetExistingUserWithIdAsNoTracking(Guid id);
    Task<User> GetExistingUserWithId(Guid id);
    Task<User> GetLoggedUserWithToken();
    void UpdateUser(User user);
    Task DeleteAccount(Guid id);
    Task AddResetPasswordCode(UserPasswordResetCode userPasswordResetCode);
    Task DeactivateExistingResetPasswordCodes(Guid userId);
    Task<UserPasswordResetCode?> GetUserResetPasswordCode(Guid userId);
    Task DeactivateAllPasswordCodes(Guid userId);
}