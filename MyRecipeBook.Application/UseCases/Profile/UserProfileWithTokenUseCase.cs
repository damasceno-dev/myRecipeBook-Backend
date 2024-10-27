using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Application.UseCases.Profile;

public class UserProfileWithTokenUseCase
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUsersRepository _usersRepository;

    public UserProfileWithTokenUseCase(ITokenProvider tokenProvider, ITokenRepository tokenRepository, IUsersRepository 
            usersRepository)
    {
        _usersRepository = usersRepository;
        _tokenProvider = tokenProvider;
        _tokenRepository = tokenRepository;
    }
    public async Task<ResponseUserProfileJson> Execute()
    {
        var token = _tokenProvider.Value();
        var id = _tokenRepository.ValidateAndGetUserIdentifier(token);
        //todo: get user with id from database
        _usersRepository.GetExistingUserWithId(id);
        //todo: response from user to responseUser
    }
}