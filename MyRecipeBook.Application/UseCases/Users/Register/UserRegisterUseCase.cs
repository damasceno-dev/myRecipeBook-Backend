using AutoMapper;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Register;

public class UserRegisterUseCase
{
    private readonly IUsersRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserRegisterUseCase(IUsersRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ResponseUserRegisterJson> Execute(RequestUserRegisterJson request)
    {
        Validate(request);
        await VerifyIfActiveUserEmailAlreadyExists(request.Email);
        
        var newUser = _mapper.Map<User>(request);
        newUser.Password = PasswordEncrypter.HashPassword(request.Password);
        
        await _repository.Register(newUser);
        await _unitOfWork.Commit();
        
        return _mapper.Map<ResponseUserRegisterJson>(newUser);
    }

    private async Task VerifyIfActiveUserEmailAlreadyExists(string newUserEmail)
    {
        var userAlreadyExists = await _repository.ExistsActiveUserWithEmail(newUserEmail);
        if (userAlreadyExists)
        {
            throw new ConflictException($"{newUserEmail} - {ResourceErrorMessages.EMAIL_ALREADY_EXISTS}");
        }
    }

    private void Validate(RequestUserRegisterJson request)
    {
        var result = new UserRegisterFluentValidation().Validate(request);
        if (result.IsValid == false)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errors);
        }
    }
}