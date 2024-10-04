using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;

namespace MyRecipeBook.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(ResponseUserRegisterJson), StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody]RequestUserRegisterJson request, 
            [FromServices]UserRegisterUseCase userRegisterUseCase)
        {
            var response = await userRegisterUseCase.Execute(request);
            return Created(string.Empty, response);
        }
    }
}
