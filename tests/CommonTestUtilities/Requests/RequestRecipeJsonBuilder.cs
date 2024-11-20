using Bogus;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Enums;

namespace CommonTestUtilities.Requests;

public class RequestRecipeJsonBuilder
{
    public static RequestRecipeJson Build()
    {
        var step = 1;
        return new Faker<RequestRecipeJson>()
            .RuleFor(recipe => recipe.Title, f => $"{f.Commerce.ProductAdjective()} {f.Commerce.ProductMaterial()} {f.Commerce.ProductName()}")
            .RuleFor(recipe => recipe.CookingTime, f => f.PickRandom<CookingTime>())
            .RuleFor(recipe => recipe.Difficulty, f => f.PickRandom<Difficulty>())
            .RuleFor(recipe => recipe.Ingredients, f => f.Make(5, () => f.Commerce.Product()))
            .RuleFor(recipe => recipe.Instructions, f => f.Make(5, i => new RequestRecipeInstructionJson
            {
                Step = step++,
                Text = f.Lorem.Sentence()
            }))
            .RuleFor(recipe => recipe.DishTypes, f => f.Make(2, f.PickRandom<DishType>));
    }
}