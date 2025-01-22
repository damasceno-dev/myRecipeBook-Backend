using AutoMapper;
using MyRecipeBook.Application.Services;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Register;

public class UserRegisterUseCase(IUsersRepository repository, IUnitOfWork unitOfWork, IMapper mapper, ITokenRepository tokenRepository, IRefreshTokenRepository refreshTokenRepository, PasswordEncryption passwordEncryption)
{
    public async Task<ResponseUserRegisterJson> Execute(RequestUserRegisterJson request)
    {
        Validate(request);
        await VerifyIfActiveUserEmailAlreadyExists(request.Email);
        
        var newUser = mapper.Map<User>(request);
        newUser.Id = Guid.NewGuid();
        newUser.Password = passwordEncryption.HashPassword(request.Password);
        var userToken = tokenRepository.Generate(newUser.Id);
        await repository.Register(newUser);
        
        var refreshToken = new RefreshToken { Value = refreshTokenRepository.Generate(), UserId = newUser.Id };
        await refreshTokenRepository.SaveRefreshToken(refreshToken);
        
        await unitOfWork.Commit();

        return new ResponseUserRegisterJson
        {
            Name = newUser.Name,
            Email = newUser.Email,
            ResponseToken = new ResponseTokenJson { Token = userToken}
        };
    }

    private async Task VerifyIfActiveUserEmailAlreadyExists(string newUserEmail)
    {
        var userAlreadyExists = await repository.ExistsActiveUserWithEmail(newUserEmail);
        if (userAlreadyExists)
        {
            throw new ConflictException($"{newUserEmail} - {ResourceErrorMessages.EMAIL_ALREADY_EXISTS}");
        }
    }

    private static void Validate(RequestUserRegisterJson request)
    {
        var result = new UserRegisterFluentValidation().Validate(request);
        if (result.IsValid == false)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new OnValidationException(errors);
        }
    }
}