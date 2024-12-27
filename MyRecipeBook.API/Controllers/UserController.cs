using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Users.ChangePassword;
using MyRecipeBook.Application.UseCases.Users.Delete;
using MyRecipeBook.Application.UseCases.Users.ExternalLogin;
using MyRecipeBook.Application.UseCases.Users.Login;
using MyRecipeBook.Application.UseCases.Users.Profile;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Application.UseCases.Users.Update;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Filters;

namespace MyRecipeBook.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
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
        public async Task<IActionResult> LoginGoogle(string returnUrl, [FromServices] UserExternalLoginUseCase userExternalLoginUse)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();

            if (authenticateResult.Succeeded is false 
                || authenticateResult.Principal is null
                || authenticateResult.Principal.Identities.Any(id => id.IsAuthenticated) is false)
                return Challenge(GoogleDefaults.AuthenticationScheme);
            
            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);
            
            if (email is null || name is null)
                return BadRequest("Couldn't get name or email from google authentication");

            var token = await userExternalLoginUse.Execute(name, email);

            return Redirect($"{returnUrl}?token={token}&name={name}&email={email}");
        }
        
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Sign out the user and clear cookies
            HttpContext.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
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
