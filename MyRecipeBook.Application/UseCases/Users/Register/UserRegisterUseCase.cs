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

    public UserRegisterUseCase(IUsersRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    public async Task<ResponseUserRegisterJson> Execute(RequestUserRegisterJson request)
    {
        Validate(request);

        var newUser = new User
        {
            Email = request.Email,
            Name = request.Name,
            Password = request.Password
        };

        _repository.Register(newUser);
        await _unitOfWork.Commit();

        return new ResponseUserRegisterJson
        {
            Id = newUser.Id,
            CreatedOn = newUser.CreatedOn,
            Name = newUser.Name,
            Email = newUser.Email,
        };
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