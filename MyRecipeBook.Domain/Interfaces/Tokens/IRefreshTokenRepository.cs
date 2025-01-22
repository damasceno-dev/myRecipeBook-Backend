using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces.Tokens;

public interface IRefreshTokenRepository
{
    string Generate();
    Task<RefreshToken?> GetRefreshToken(string token);
    Task SaveRefreshToken(RefreshToken refreshToken);
}