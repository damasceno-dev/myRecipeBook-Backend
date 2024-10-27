namespace MyRecipeBook.Domain.Interfaces.Tokens;

public interface ITokenRepository
{
    string Generate(Guid specificGuid);
    Guid ValidateAndGetUserIdentifier(string token);
}