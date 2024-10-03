using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Infrastructure.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public UsersRepository(MyRecipeBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public void Register(User newUser)
    {
        _dbContext.Users.Add(newUser);
    }
}