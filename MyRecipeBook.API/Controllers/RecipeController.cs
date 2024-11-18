using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Recipes.DeleteById;
using MyRecipeBook.Application.UseCases.Recipes.Filter;
using MyRecipeBook.Application.UseCases.Recipes.GetById;
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
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> FilterRecipes([FromBody] RequestRecipeFilterJson requestRecipeFilter, [FromServices] RecipeFilterUseCase filterUseCase)
        {
            var response = await filterUseCase.Execute(requestRecipeFilter);
            if (response.Count != 0)
                return Ok(response);

            return NoContent();
        }

        [HttpGet]
        [Route("getById/{recipeId}")]
        [ProducesResponseType(typeof(ResponseRecipeJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById([FromRoute] Guid recipeId, [FromServices]RecipeGetByIdUseCase getByIdUseCase)
        {
            var response = await getByIdUseCase.Execute(recipeId);
            return Ok(response);
        }

        [HttpDelete]
        [Route("deleteById/{recipeId}")]
        [ProducesResponseType(typeof(ResponseRecipeJson), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid recipeId, [FromServices] RecipeDeleteByIdUseCase deleteByIdUseCase)
        {
            await deleteByIdUseCase.Execute(recipeId);
            return NoContent();
        }
        
    }
}

