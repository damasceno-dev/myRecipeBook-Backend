using Bogus;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace CommonTestUtilities.Entities;

public class RecipeBuilder
{
    public static List<Recipe> RecipeCollection(User user, uint count = 10, double imageIdentifierPercentage = 0.0)
    {
        if (count <= 0)
            count = 2;

        var recipes = new List<Recipe>();
        var faker = new Faker();

        for (var i = 0; i < count; i++)
        {
            var recipe = Build(user);
            
            // Assign an ImageIdentifier based on the percentage
            if (faker.Random.Double(0, 1) <= imageIdentifierPercentage)
            {
                recipe.ImageIdentifier = faker.Random.Bool() ? $"{Guid.NewGuid()}.jpg" : $"{Guid.NewGuid()}.png";
            }

            recipes.Add(recipe);
        }

        return recipes;
    }
    public static Recipe Build(User user)
    {
        return new Faker<Recipe>()
            .RuleFor(recipe => recipe.Id, Guid.NewGuid)
            .RuleFor(recipe => recipe.Title, f => $"{f.Commerce.ProductAdjective()} {f.Commerce.ProductMaterial()} {f.Commerce.ProductName()}")
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