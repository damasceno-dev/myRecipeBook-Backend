using Bogus;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace CommonTestUtilities.Entities;

public class RecipeBuilder
{
    public static Recipe Build(User user)
    {
        return new Faker<Recipe>()
            .RuleFor(recipe => recipe.Id, Guid.NewGuid)
            .RuleFor(recipe => recipe.Title, (f) => f.Lorem.Word())
            .RuleFor(recipe => recipe.CookingTime, (f) => f.PickRandom<CookingTime>())
            .RuleFor(recipe => recipe.Difficulty, (f) => f.PickRandom<Difficulty>())
            .RuleFor(recipe => recipe.Ingredients, (f) => f.Make(f.Random.Number(1, 5), () => new Ingredient
            {
                Id = Guid.NewGuid(),
                Item = f.Commerce.ProductName()
            }))
            .RuleFor(recipe => recipe.Instructions, (f) => f.Make(f.Random.Number(1, 5), () => new Instruction
            {
                Id = Guid.NewGuid(),
                Step = f.IndexFaker + 1, // Increment step number for each instruction
                Text = f.Lorem.Paragraph()
            }))
            .RuleFor(recipe => recipe.DishTypes, (f) => f.Make(f.Random.Number(1, 3), () => new MyRecipeBook.Domain.Entities.DishType
            {
                Id = Guid.NewGuid(),
                Type = f.PickRandom<DishType>()
            }))
            .RuleFor(recipe => recipe.UserId, _ => user.Id);
    }
}