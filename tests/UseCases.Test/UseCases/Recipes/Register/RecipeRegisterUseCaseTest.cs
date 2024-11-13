using CommonTestUtilities.Entities;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Application.UseCases.Recipes.Register;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Exception;
using Xunit;
using DishType = MyRecipeBook.Domain.Enums.DishType;

namespace UseCases.Test.UseCases.Recipes.Register;

public class RecipeRegisterUseCaseTest
{
    
    [Fact]
    public async Task Success()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        var useCase = CreateRecipeRegisterUseCase();
        var response =  await useCase.Execute(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Title.Should().Be(request.Title);
    }

        [Fact]
    public async void RecipeTitleEmpty()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Title = string.Empty;
        
        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
       
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_TITLE_EMPTY);
    }
    
    [Fact]
    public async void RecipeNotInEnum()
    {
        var outOfRangeDishType = (DishType)EnumTestHelper.OutOfRangeEnum<DishType>();
        var outOfRangeDifficulty = (Difficulty)EnumTestHelper.OutOfRangeEnum<Difficulty>();
        var outOfRangeCookingTime = (CookingTime)EnumTestHelper.OutOfRangeEnum<CookingTime>();
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.DishTypes.Add(outOfRangeDishType);
        request.Difficulty = outOfRangeDifficulty;
        request.CookingTime = outOfRangeCookingTime;
        
        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
       

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().HaveCount(3);
        exception.And.GetErrors.Should().Contain(new[]
        {
            ResourceErrorMessages.RECIPE_DIFFICULTY_NOT_IN_ENUM,
            ResourceErrorMessages.RECIPE_COOKING_TIME_NOT_IN_ENUM,
            ResourceErrorMessages.RECIPE_DISH_TYPE_NOT_IN_ENUM
        });
    }

    [Fact]
    public async void RecipeIngredientListEmpty()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Ingredients = [];
        
        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);

        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_AT_LEAST_ONE_INGREDIENT);
    }
    
    [Fact]
    public async void RecipeIngredientItemEmpty()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Ingredients = ["    "];
        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
       
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_INGREDIENT_NOT_EMPTY);
    }
    
    [Fact]
    public async void RecipeInstructionStepGreaterThanZero()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 0, Text = "Chop onions" }
        };
        
        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
        
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_INSTRUCTION_STEP_GREATER_THAN_0);
    }

    [Fact]
    public async void RecipeInstructionTextNotEmpty()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = string.Empty }
        };

        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_INSTRUCTION_TEXT_NOT_EMPTY);
    }

    [Fact]
    public async void RecipeInstructionTextLessThan2000Characters()
    {
        var overLimitText = new string('a', SharedValidators.MaximumRecipeInstructionTextLength + 1);
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = overLimitText }
        };

        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_INSTRUCTION_TEXT_LESS_THAN_2000);
    }

    [Fact]
    public async void RecipeInstructionStepMustBeUnique()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        request.Instructions = new List<RequestRecipeInstructionJson>
        {
            new() { Step = 1, Text = "Step 1" },
            new() { Step = 1, Text = "Duplicate Step 1" }
        };

        var useCase = CreateRecipeRegisterUseCase();
        var act = async () => await useCase.Execute(request);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.RECIPE_INSTRUCTION_DUPLICATE_STEP_INSTRUCTION);
    }


    private static RecipeRegisterUseCase CreateRecipeRegisterUseCase()
    {
        var (user, _) = UserBuilder.Build();
        var mapper = MapperBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var recipeRepository = new RecipeRepositoryBuilder().Build();
        return new RecipeRegisterUseCase(mapper, unitOfWork, usersRepository, recipeRepository);
    }
}