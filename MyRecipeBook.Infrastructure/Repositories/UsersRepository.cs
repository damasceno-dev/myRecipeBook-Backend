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

    public async Task<User?> GetExistingUserWithIdAsNoTracking(Guid Id)
    {
        return await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.Equals(Id));
    }

    public async Task<User> GetExistingUserWithId(Guid Id)
    {
        return await dbContext.Users.FirstAsync(u => u.Id.Equals(Id));
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

    public async Task DeleteAccount(Guid Id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == Id);
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
    
    public async Task DeactivateExistingResetPasswordCodes(Guid userId)
    {
        var existingCodes = await dbContext.UserPasswordResetCodes
            .Where(code => code.UserId == userId && code.Active)
            .ToListAsync();

        foreach (var code in existingCodes)
        {
            code.Active = false;
        }
    }

    public async Task<UserPasswordResetCode?> GetUserResetPasswordCode(Guid userId)
    {
        return await dbContext.UserPasswordResetCodes.FirstOrDefaultAsync(code => code.UserId == userId && code.Active == true);
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