using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly MyRecipeBookDbContext _dbContext;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITokenRepository _tokenRepository;

    public UsersRepository(MyRecipeBookDbContext dbContext, ITokenProvider tokenProvider, ITokenRepository tokenRepository)
    {
        _dbContext = dbContext;
        _tokenProvider = tokenProvider;
        _tokenRepository = tokenRepository;
    }
    public async Task Register(User newUser)
    {
        await _dbContext.Users.AddAsync(newUser);
    }
    public async Task<bool> ExistsActiveUserWithEmail(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email.Equals(email) && u.Active);
    }

    public async Task<User?> GetExistingUserWithEmail(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
    }

    public async Task<User?> GetExistingUserWithIdAsNoTracking(Guid id)
    {
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.Equals(id));
    }

    public async Task<User> GetExistingUserWithId(Guid id)
    {
        return await _dbContext.Users.FirstAsync(u => u.Id.Equals(id));
    }

    public Task<User> GetLoggedUserWithToken()
    {
        var token = _tokenProvider.Value();
        var userId = _tokenRepository.ValidateAndGetUserIdentifier(token);
        return _dbContext.Users.AsNoTracking().FirstAsync(u => u.Id.Equals(userId));
    }

    public void UpdateUser(User user)
    {
        _dbContext.Users.Update(user);
    }
}