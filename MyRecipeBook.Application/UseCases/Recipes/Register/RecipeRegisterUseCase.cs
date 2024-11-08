using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipes.Register;

public class RecipeRegisterUseCase
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsersRepository _usersRepository;
    private readonly IRecipesRepository _recipeRepository;

    public RecipeRegisterUseCase(IMapper mapper, IUnitOfWork unitOfWork, IUsersRepository usersRepository, IRecipesRepository recipeRepository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _usersRepository = usersRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<ResponseRecipeJson> Execute(RequestRecipeJson request)
    {
        Validate(request);
        
        var user = await _usersRepository.GetLoggedUserWithToken();
        var recipe = _mapper.Map<Recipe>(request);
        recipe.UserId = user.Id;
        
        await _recipeRepository.Register(recipe);
        await _unitOfWork.Commit();
        
        return _mapper.Map<ResponseRecipeJson>(recipe);
        
    }

    private void Validate(RequestRecipeJson request)
    {
        var result = new RecipeRegisterFluentValidation().Validate(request);
        if (result.IsValid is false)
        {
            var erros = result.Errors.Select(x => x.ErrorMessage).ToList();
            throw new OnValidationException(erros);
        }
    }
}