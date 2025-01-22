using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Infrastructure.Repositories;

public class RefreshTokenRepository(MyRecipeBookDbContext dbContext) : IRefreshTokenRepository
{
    public string Generate() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    public async Task<RefreshToken?> GetRefreshToken(string refreshToken)
    {
        return await dbContext
            .RefreshTokens
            .AsNoTracking()
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.Value.Equals(refreshToken));
    }

    public async Task SaveRefreshToken(RefreshToken refreshToken)
    {
        var tokens = dbContext.RefreshTokens.Where(token => token.UserId == refreshToken.UserId);

        dbContext.RefreshTokens.RemoveRange(tokens);

        await dbContext.RefreshTokens.AddAsync(refreshToken);
    }
}