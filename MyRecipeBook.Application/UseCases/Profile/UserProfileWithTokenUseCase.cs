using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Application.UseCases.Profile;

public class UserProfileWithTokenUseCase
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IUsersRepository _usersRepository;

    public UserProfileWithTokenUseCase(ITokenProvider tokenProvider, IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
        _tokenProvider = tokenProvider;
    }
    public async Task<ResponseUserProfileJson> Execute()
    {
        var token = _tokenProvider.Value();
        //todo: token validation (and get id)
        var id = _tokenValidator.Validate(token);
        //todo: get user with id from database
        _usersRepository.GetExistingUserWithId(id);
        //todo: response from user to responseUser
    }
}