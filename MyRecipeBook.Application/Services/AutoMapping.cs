using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using DishType = MyRecipeBook.Domain.Entities.DishType;

namespace MyRecipeBook.Application.Services;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        RequestToDomain();
        DomainToResponse();
    }

    private void DomainToResponse()
    {
        CreateMap<User, ResponseUserRegisterJson>();
        CreateMap<User, ResponseUserProfileJson>();
        CreateMap<Recipe, ResponseRegisteredRecipeJson>();
        CreateMap<Recipe, ResponseShortRecipeJson>()
            .ForMember(dest => dest.QuantityIngredients, 
                config => config.MapFrom(source => source.Ingredients.Count))
            .ForMember(dest => dest.ImageUrl,
                config => config.MapFrom(source => source.ImageIdentifier));
        CreateMap<Recipe, ResponseRecipeJson>()
            .ForMember(dest => dest.DishTypes,
                config => config.MapFrom(source => source.DishTypes.Select(d => d.Type).ToList()))
            .ForMember(dest => dest.Ingredients,
                config => config.MapFrom(source => source.Ingredients.Select(i => i.Item).ToList()))
            .ForMember(dest => dest.Instructions,
                config => config.MapFrom(source => source.Instructions.Select(i => new ResponseInstructionJson { Step = i.Step, Text = i.Text }).ToList()));
        CreateMap<RecipeDto, ResponseRecipeGeneratedJson>()
            .ForMember(dest => dest.DishTypes,
                config => config.MapFrom(source => source.DishTypes.Select(d => d.Type).ToList()))
            .ForMember(dest => dest.Ingredients,
                config => config.MapFrom(source => source.Ingredients.Select(i => i.Item).ToList()))
            .ForMember(dest => dest.Instructions,
                config => config.MapFrom(source => source.Instructions.Select(i => new ResponseInstructionJson {Step = i.Step, Text = i.Text}).ToList()));
    }

    private void RequestToDomain()
    {
        CreateMap<RequestUserRegisterJson, User>()
            .ForMember(u => u.Password, config => config.Ignore());
        CreateMap<RequestRecipeJson, Recipe>().ConvertUsing(MapRequestToRecipe);
        CreateMap<RequestRecipeFilterJson, FilterRecipeDto>().ConvertUsing(request => MapRequestRecipeFilterToFilterDto(request));
    }

    private static FilterRecipeDto MapRequestRecipeFilterToFilterDto(RequestRecipeFilterJson requestRecipe)
    {
        var titleIngredient = requestRecipe.TitleIngredient;
        var cookingTimes = requestRecipe.CookingTimes.Distinct().ToList();
        var difficulties = requestRecipe.Difficulties.Distinct().ToList();
        var dishTypes = requestRecipe.DishTypes.Distinct().ToList();
        return new FilterRecipeDto(titleIngredient, cookingTimes, difficulties, dishTypes);
    }
    
    /// <summary>
    /// Method used for both registering and updating a recipe
    /// </summary>
    /// <param name="request">New recipe to register or update</param>
    /// <param name="existingRecipe">If there is an existing recipe, it updates it.
    /// Otherwise, it creates a new one.</param>
    /// <returns>Recipe object</returns>
    private static Recipe MapRequestToRecipe(RequestRecipeJson request, Recipe? existingRecipe = null)
    {
        var recipe = existingRecipe ?? new Recipe
        {
            Id = Guid.NewGuid()
        };

        // Map primitive properties
        recipe.Title = request.Title;
        recipe.CookingTime = request.CookingTime;
        recipe.Difficulty = request.Difficulty;

        // Map Ingredients
        recipe.Ingredients = request.Ingredients
            .Select(ingredient => new Ingredient { Item = ingredient })
            .ToList();

        // Map Instructions with ordering
        recipe.Instructions = request.Instructions
            .OrderBy(instruction => instruction.Step)
            .Select((instruction, index) => new Instruction
            {
                Step = index + 1,
                Text = instruction.Text
            })
            .ToList();

        // Map DishTypes
        recipe.DishTypes = request.DishTypes
            .Select(dishType => new DishType { Type = dishType })
            .ToList();

        return recipe;
    }

}