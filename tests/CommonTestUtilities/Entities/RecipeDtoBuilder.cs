using Bogus;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace CommonTestUtilities.Entities;

public class RecipeDtoBuilder
{
    public static RecipeDto Build(IList<string>? ingredients = null)
    {
        var faker = new Faker();

        
        var title = $"{faker.Commerce.ProductAdjective()} {faker.Commerce.ProductMaterial()} {faker.Commerce.ProductName()}";
        var cookingTime = faker.PickRandom<CookingTime>();
        var difficulty = faker.PickRandom<Difficulty>();
        var ingredientsList = ingredients is not null ? 
            ingredients.Select(item => new Ingredient
            {
                Id = Guid.NewGuid(),
                Item = item
            }).ToList() 
                : 
            faker.Make(faker.Random.Number(1, 5), () => new Ingredient
            {
                Id = Guid.NewGuid(),
                Item = faker.Commerce.ProductName()
            }).ToList();
        var instructions = faker.Make(faker.Random.Number(1, 5), () => new Instruction
        {
            Id = Guid.NewGuid(),
            Step = faker.IndexFaker + 1, // Increment step number for each instruction
            Text = faker.Lorem.Paragraph()
        }).ToList();
        var dishTypes = faker.Make(faker.Random.Number(1, 3), () => new MyRecipeBook.Domain.Entities.DishType
        {
            Id = Guid.NewGuid(),
            Type = faker.PickRandom<DishType>()
        }).ToList();

        return new RecipeDto(title, cookingTime, difficulty, ingredientsList, instructions, dishTypes);
    }
}