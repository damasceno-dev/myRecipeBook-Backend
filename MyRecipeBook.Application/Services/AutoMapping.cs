using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;

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
    }

    private void RequestToDomain()
    {
        CreateMap<RequestUserRegisterJson, User>()
            .ForMember(u => u.Password, config => config.Ignore());
        CreateMap<RequestRecipeJson, Recipe>().ConvertUsing(request => MapRequestToRecipe(request));
            
    }
    private static Recipe MapRequestToRecipe(RequestRecipeJson request)
    {
        return new Recipe
        {
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