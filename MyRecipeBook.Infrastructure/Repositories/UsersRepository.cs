using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public UsersRepository(MyRecipeBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Register(User newUser)
    {
        await _dbContext.Users.AddAsync(newUser);
    }
}