using AutoMapper;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;

namespace MyRecipeBook.Application.UseCases.Profile;

public class UserProfileWithTokenUseCase
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;

    public UserProfileWithTokenUseCase(ITokenProvider tokenProvider, ITokenRepository tokenRepository, IUsersRepository 
            usersRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _tokenProvider = tokenProvider;
        _tokenRepository = tokenRepository;
        _mapper = mapper;
    }
    public async Task<ResponseUserProfileJson> Execute()
    {
        var token = _tokenProvider.Value();
        var id = _tokenRepository.ValidateAndGetUserIdentifier(token);
        var user = await _usersRepository.GetExistingUserWithId(id);
        var verifiedUser = VerifyUser(user);
        return _mapper.Map<ResponseUserProfileJson>(verifiedUser);
    }

    private static User VerifyUser(User? user)
    {
        if (user is null)
        {
            throw new ArgumentException("User does not exists");
        }
        return user;
    }
}