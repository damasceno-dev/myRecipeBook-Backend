using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Recipes.Filter;
using MyRecipeBook.Application.UseCases.Recipes.Register;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Filters;

namespace MyRecipeBook.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [MyCustomAuthorize]
    public class RecipeController : ControllerBase
    {
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterRecipe([FromBody]RequestRecipeJson requestRecipe, [FromServices]RecipeRegisterUseCase registerUseCase)
        {
            var response = await registerUseCase.Execute(requestRecipe);
            return Created(string.Empty, response);
        }

        [HttpPost]
        [Route("filter")]
        [ProducesResponseType(typeof(List<ResponseShortRecipeJson>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> FilterRecipes([FromBody] RequestRecipeFilterJson requestRecipeFilter, [FromServices] RecipeFilterUseCase filterUseCase)
        {
            var response = await filterUseCase.Execute(requestRecipeFilter);
            if (response.Count != 0)
                return Ok(response);

            return NoContent();
        }
    }
}
