using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Interfaces;

public interface IRecipesRepository
{
    Task Register(Recipe recipe);
}