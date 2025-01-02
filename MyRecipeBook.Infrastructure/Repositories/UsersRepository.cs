using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Infrastructure.Repositories;

internal class UsersRepository(MyRecipeBookDbContext dbContext, ITokenProvider tokenProvider, ITokenRepository tokenRepository)
    : IUsersRepository
{
    public async Task Register(User newUser)
    {
        await dbContext.Users.AddAsync(newUser);
    }
    public async Task<bool> ExistsActiveUserWithEmail(string email)
    {
        return await dbContext.Users.AnyAsync(u => u.Email.Equals(email) && u.Active);
    }

    public async Task<User?> GetExistingUserWithEmail(string email)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
    }

    public async Task<User?> GetExistingUserWithIdAsNoTracking(Guid id)
    {
        return await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.Equals(id));
    }

    public async Task<User> GetExistingUserWithId(Guid id)
    {
        return await dbContext.Users.FirstAsync(u => u.Id.Equals(id));
    }

    public Task<User> GetLoggedUserWithToken()
    {
        var token = tokenProvider.Value();
        var userId = tokenRepository.ValidateAndGetUserIdentifier(token);
        return dbContext.Users.AsNoTracking().FirstAsync(u => u.Id.Equals(userId));
    }

    public void UpdateUser(User user)
    {
        dbContext.Users.Update(user);
    }

    public async Task DeleteAccount(Guid id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user is null)
            return;

        var recipes = dbContext.Recipes.Where(recipe => recipe.UserId == user.Id);

        dbContext.Recipes.RemoveRange(recipes);

        dbContext.Users.Remove(user);
    }

    public async Task AddResetPasswordCode(UserPasswordResetCode userPasswordResetCode)
    {
        await dbContext.UserPasswordResetCodes.AddAsync(userPasswordResetCode);
    }
    
    public async Task DeactivateExistingResetPasswordCodes(Guid id)
    {
        var existingCodes = await dbContext.UserPasswordResetCodes
            .Where(code => code.UserId == id && code.Active)
            .ToListAsync();

        foreach (var code in existingCodes)
        {
            code.Active = false;
        }
    }

    public async Task<UserPasswordResetCode?> GetUserResetPasswordCode(Guid id)
    {
        return await dbContext.UserPasswordResetCodes.FirstOrDefaultAsync(code => code.UserId == id && code.Active == true);
    }

    public async Task DeactivateAllPasswordCodes(Guid userId)
    {
        var existingCodes = await dbContext.UserPasswordResetCodes
            .Where(code => code.UserId == userId)
            .ToListAsync();

        foreach (var code in existingCodes)
        {
            code.Active = false;
        }
    }
}