using CommonTestUtilities.Entities;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Services;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.GenerateWithAI;

public class RecipeGenerateWithAIUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);
        
        var response =  await useCase.Execute(request);

        response.Should().NotBeNull();
        response.Should().NotBeNull();
        response.Title.Should().NotBeNullOrWhiteSpace();
        response.CookingTime.Should().NotBeNull();
        response.Difficulty.Should().NotBeNull();
        response.Ingredients.Should().NotBeNullOrEmpty();
        response.Ingredients.Should().BeEquivalentTo(request.Ingredients, options => options.WithStrictOrdering());
        response.Instructions.Should().NotBeNullOrEmpty();
        response.DishTypes.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task ErrorIngredientListCountExceeded()
    {
        var numberOfIngredients = SharedValidators.MaximumRecipeIngredients + new Random().Next(1, 20);
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build(numberOfIngredients);
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);

        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(error => error == ResourceErrorMessages.RECIPE_INGREDIENT_LIST_MAXIMUM_COUNT);
    }

    [Fact]
    public async Task ErrorIngredientEmpty()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = string.Empty;
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);

        var act = async () => await useCase.Execute(request);

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(error => error == ResourceErrorMessages.RECIPE_INGREDIENT_NOT_EMPTY);
    }

    [Fact]
    public async Task ErrorInvalidSeparators()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1-cup-flour";
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);

        var act = async () => await useCase.Execute(request);

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(error => error == ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_SEPARATORS);
    }

    [Fact]
    public async Task ErrorInvalidStartCharacter()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "-flour";
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);

        var act = async () => await useCase.Execute(request);

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(error => error == ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_START_CHARACTER);
    }

    [Fact]
    public async Task ErrorMaximumWordCountExceeded()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1 cup of very very very fine flour";
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);

        var act = async () => await useCase.Execute(request);

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(error => error == ResourceErrorMessages.RECIPE_INGREDIENT_MAXIMUM_WORD_COUNT);
    }

    [Fact]
    public async Task ErrorInvalidCharacter()
    {
        var request = RequestRecipeIngredientsForAIJsonBuilder.Build();
        request.Ingredients[0] = "1 cup @ flour";
        var recipeDto = RecipeDtoBuilder.Build(request.Ingredients);
        var useCase = CreateRecipeGenerateWithAIUseCase(recipeDto);

        var act = async () => await useCase.Execute(request);

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(error => error == ResourceErrorMessages.RECIPE_INGREDIENT_INVALID_CHARACTER);
    }
    
    private static RecipeGenerateWithAIUseCase CreateRecipeGenerateWithAIUseCase(RecipeDto recipe)
    {
        var mapper = MapperBuilder.Build();
        var generateWithAIService = new ChatGptServiceBuilder().GenerateAIRecipe(recipe).Build();
        
        return new RecipeGenerateWithAIUseCase(generateWithAIService,mapper);
    }
}