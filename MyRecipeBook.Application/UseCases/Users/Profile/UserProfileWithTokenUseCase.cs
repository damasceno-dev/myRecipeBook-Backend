using AutoMapper;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.Profile;

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
        //user null verification is already done in the authorization filter
        var verifiedUser = VerifyUser(user!);
        return _mapper.Map<ResponseUserProfileJson>(verifiedUser);
    }

    private static User VerifyUser(User user)
    {
        if (user.Active is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
        }
        return user;
    }
}