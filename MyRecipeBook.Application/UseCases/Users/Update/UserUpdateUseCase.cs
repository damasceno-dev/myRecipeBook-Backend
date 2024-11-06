using AutoMapper;
using Microsoft.Extensions.Options;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Update;

public class UserUpdateUseCase
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserUpdateUseCase(IUsersRepository usersRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ResponseUserProfileJson> Execute(RequestUserUpdateJson request)
    {
        var user = await _usersRepository.GetLoggedUserWithToken();
        
        Validate(request);
        await ValidateIfEmailAlreadyExists(request.Email);
        
        user.Name = request.Name;
        user.Email = request.Email;
        
        _usersRepository.UpdateUser(user);
        await _unitOfWork.Commit();
        
        return _mapper.Map<ResponseUserProfileJson>(user);
    }

    private async Task ValidateIfEmailAlreadyExists(string requestEmail)
    {
        var userWithSameEmail = await _usersRepository.GetExistingUserWithEmail(requestEmail);
        if (userWithSameEmail is not null)
        {
            throw new ConflictException(ResourceErrorMessages.EMAIL_ALREADY_EXISTS);
        }
    }

    private static void Validate(RequestUserUpdateJson request)
    {
        var validation = new UserUpdateFluentValidation().Validate(request);
        if (!validation.IsValid)
        {
            var errorMessages = validation.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errorMessages);
        }
    }
}