using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Users.ChangePassword;
using MyRecipeBook.Application.UseCases.Users.Delete;
using MyRecipeBook.Application.UseCases.Users.ExternalLogin;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Application.UseCases.Users.Profile;
using MyRecipeBook.Application.UseCases.Users.RefreshTokens;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Application.UseCases.Users.ResetPassword;
using MyRecipeBook.Application.UseCases.Users.Update;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Filters;
namespace MyRecipeBook.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController(ILogger<UserController> logger)
        : ControllerBase
    {
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(ResponseUserRegisterJson), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody]RequestUserRegisterJson request, 
            [FromServices]UserRegisterUseCase userRegisterUseCase)
        {
            var response = await userRegisterUseCase.Execute(request);
            return Created(string.Empty, response);
        }
        
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(typeof(ResponseUserLoginJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DoLogin([FromBody]RequestUserLoginJson request, [FromServices]UserLoginUseCase
            userLoginUseCase)
        {
            var response = await userLoginUseCase.Execute(request);
            return Ok(response);
        }
	
        [HttpGet("login/google")]
        public async Task<IActionResult> LoginGoogle(string returnUrl, [FromServices] UserExternalLoginUseCase userExternalLoginUse, [FromServices] IConfiguration configuration)
        {
            logger.LogInformation("Received returnUrl: {ReturnUrl}", returnUrl);
            
            var allowedReturnUrls = configuration.GetSection("Settings:ExternalLogin:AllowedReturnUrls").Get<string[]>();
            logger.LogInformation("Allowed URLs: {AllowedUrls}", string.Join(", ", allowedReturnUrls ?? []));
            // List of allowed return URLs for security reasons
            
            if (string.IsNullOrWhiteSpace(returnUrl) || (allowedReturnUrls ?? throw new InvalidOperationException()).Contains(returnUrl) is false)
            {
                returnUrl = "/"; // Default safe redirect
            }
            
            var authenticateResult = await HttpContext.AuthenticateAsync();

            if (authenticateResult.Succeeded is false 
                || authenticateResult.Principal is null
                || authenticateResult.Principal.Identities.Any(id => id.IsAuthenticated) is false)
                return Challenge(GoogleDefaults.AuthenticationScheme);
            
            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);
            
            if (email is null || name is null)
                return BadRequest("Couldn't get name or email from google authentication");

            var responseUserLogin = await userExternalLoginUse.Execute(name, email);
            var url = $"{returnUrl}?token={responseUserLogin.ResponseToken.Token}&name={responseUserLogin.Name}&email={responseUserLogin.Email}&refreshToken={responseUserLogin.ResponseToken.RefreshToken}";
            return Redirect(url);
        }
        
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ResponseSuccessLogoutJson), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
                logger.LogInformation("Logout process started.");
                // Sign out the local authentication cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                logger.LogInformation("Logout completed successfully.");
                return Ok(new ResponseSuccessLogoutJson("Logged out successfully"));
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ResponseTokenJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromServices] UserRefreshTokenUseCase userRefreshTokenUseCase, [FromBody] RequestRefreshTokenJson refreshToken)
        {
            var response = await userRefreshTokenUseCase.Execute(refreshToken);
            return Ok(response);
        }
        
        [HttpGet]
        [Route("get-reset-password-code/{email}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> GetResetPasswordCode([FromServices] UserGetResetPasswordCodeUseCase userGetResetPasswordCodeUseCase, [FromRoute] string email)
        {
            await userGetResetPasswordCodeUseCase.Execute(email);
            return Accepted();
        }
        
        [HttpPost]
        [Route("reset-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetUserPassword([FromServices] UserResetPasswordUseCase userResetPasswordUseCase, [FromBody] RequestUserResetPasswordJson requestUser)
        {
            await userResetPasswordUseCase.Execute(requestUser);
            return NoContent();
        }

        [HttpGet]
        [MyCustomAuthorize]
        [Route("getProfileWithToken")]
        [ProducesResponseType(typeof(ResponseUserProfileJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfileWithToken(
            [FromServices] UserProfileWithTokenUseCase userProfileWithTokenUseCase)
        {
            var response = await userProfileWithTokenUseCase.Execute();
            return Ok(response);
        }

        [HttpPut]
        [MyCustomAuthorize]
        [Route("update")]
        [ProducesResponseType(typeof(ResponseUserProfileJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromServices] UserUpdateUseCase userUpdateUseCase,
            [FromBody] RequestUserUpdateJson request)
        {
            var response = await userUpdateUseCase.Execute(request);
            return Ok(response);
        }

        [HttpPut]
        [MyCustomAuthorize]
        [Route("changePassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ChangePassword([FromServices] UserChangePasswordUseCase userChangePasswordUseCase, [FromBody] RequestUserChangePasswordJson request)
        {
            await userChangePasswordUseCase.Execute(request);
            return NoContent();
        }
        
        [HttpDelete]
        [MyCustomAuthorize]
        [Route("delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> QueueUserDeletion([FromServices]UserQueueDeletionUseCase userQueueDeletion)
        {
            await userQueueDeletion.Execute();
            return NoContent();
        }
    }

}
