namespace MyRecipeBook.Domain.Interfaces;

public interface ITokenGenerator
{
    string Generate(Guid specificGuid);
}