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
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;

    public UserProfileWithTokenUseCase( IUsersRepository usersRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
    }
    public async Task<ResponseUserProfileJson> Execute()
    {
        var user = await _usersRepository.GetLoggedUserWithToken();
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