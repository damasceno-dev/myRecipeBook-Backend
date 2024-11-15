using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
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
        CreateMap<Recipe, ResponseRecipeJson>();
        CreateMap<Recipe, ResponseShortRecipeJson>()
            .ForMember(dest => dest.QuantityIngredients, 
                config => config.MapFrom(source => source.Ingredients.Count));
    }

    private void RequestToDomain()
    {
        CreateMap<RequestUserRegisterJson, User>()
            .ForMember(u => u.Password, config => config.Ignore());
        CreateMap<RequestRecipeJson, Recipe>().ConvertUsing(request => MapRequestToRecipe(request));
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
    private static Recipe MapRequestToRecipe(RequestRecipeJson request)
    {
        return new Recipe
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            CookingTime = request.CookingTime,
            Difficulty = request.Difficulty,
        
            Ingredients = request.Ingredients.Select(ingredient => new Ingredient { Item = ingredient }).ToList(),
            Instructions = request.Instructions.Select(instruction => new Instruction
            {
                Step = instruction.Step,
                Text = instruction.Text
            }).ToList(),
            DishTypes = request.DishTypes.Select(dishType => new DishType { Type = dishType }).ToList()
        };
    }

}