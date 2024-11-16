using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Users.ChangePassword;
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
    }
}
