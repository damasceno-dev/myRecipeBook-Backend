using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Communication;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Filters;

public class MyCustomAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ITokenRepository _tokenRepository;
    private readonly IUsersRepository _usersRepository;

    public MyCustomAuthorizeFilter(ITokenProvider tokenProvider, ITokenRepository tokenRepository, IUsersRepository 
            usersRepository)
    {
        _tokenProvider = tokenProvider;
        _tokenRepository = tokenRepository;
        _usersRepository = usersRepository;
    }
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            var token = _tokenProvider.Value();

            var userId = _tokenRepository.ValidateAndGetUserIdentifier(token);
            var user = await _usersRepository.GetExistingUserWithIdAsNoTracking(userId);
            if (user is null)
            {
                throw new UnauthorizedAccessException(ResourceErrorMessages.TOKEN_WITH_NO_PERMISSION);
            }
        }
        catch (TokenEmptyException)
        {
            context.Result = new UnauthorizedObjectResult(new ResponseErrorJson(ResourceErrorMessages.TOKEN_EMPTY));
        }
        catch (SecurityTokenExpiredException)
        {
            context.Result = new UnauthorizedObjectResult(new ResponseErrorJson(ResourceErrorMessages.TOKEN_EXPIRED));
        }
        catch (MyRecipeBookException myRecipeBookException)
        {
            context.HttpContext.Response.StatusCode = myRecipeBookException.GetStatusCode;
            context.Result = new ObjectResult(new ResponseErrorJson(myRecipeBookException.GetErrors));
        }
        catch
        {
            context.Result = new UnauthorizedObjectResult(new ResponseErrorJson(ResourceErrorMessages.TOKEN_WITH_NO_PERMISSION));
        }
    }
}