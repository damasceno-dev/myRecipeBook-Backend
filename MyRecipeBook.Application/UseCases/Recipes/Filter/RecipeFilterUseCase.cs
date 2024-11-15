using AutoMapper;
using Microsoft.Extensions.Options;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.Filter;

public class RecipeFilterUseCase
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRecipesRepository _recipesRepository;
    private readonly IMapper _mapper;

    public RecipeFilterUseCase(IUsersRepository usersRepository, IRecipesRepository recipesRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _recipesRepository = recipesRepository;
        _mapper = mapper;
    }

    public async Task<List<ResponseShortRecipeJson>> Execute(RequestRecipeFilterJson requestRecipe)
    {
        Validate(requestRecipe);

        var user = await _usersRepository.GetLoggedUserWithToken();
        var filter = _mapper.Map<FilterRecipeDto>(requestRecipe);
        var recipesFiltered = await _recipesRepository.FilterRecipe(user,filter);

        return recipesFiltered.Select(recipe => _mapper.Map<ResponseShortRecipeJson>(recipe)).ToList();
    }

    private static void Validate(RequestRecipeFilterJson requestRecipe)
    {
        var result = new RecipeFilterFluentValidation().Validate(requestRecipe);
        if (result.IsValid is false)
        {
            var erros = result.Errors.Select(x => x.ErrorMessage).ToList();
            throw new OnValidationException(erros);
        }
    }
}