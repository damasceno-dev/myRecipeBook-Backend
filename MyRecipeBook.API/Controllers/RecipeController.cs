using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Application.UseCases.Recipes.DeleteById;
using MyRecipeBook.Application.UseCases.Recipes.Filter;
using MyRecipeBook.Application.UseCases.Recipes.GenerateWithAI;
using MyRecipeBook.Application.UseCases.Recipes.GetById;
using MyRecipeBook.Application.UseCases.Recipes.GetByUser;
using MyRecipeBook.Application.UseCases.Recipes.ImageUpdateCover;
using MyRecipeBook.Application.UseCases.Recipes.Register;
using MyRecipeBook.Application.UseCases.Recipes.Update;
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
        [ProducesResponseType(typeof(ResponseRecipeJson), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterRecipe([FromForm] RequestRecipeForm requestRecipe, [FromServices]RecipeRegisterUseCase registerUseCase)
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
        [Route("getById/{recipeId:guid}")]
        [ProducesResponseType(typeof(ResponseRecipeJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById([FromRoute]Guid recipeId, [FromServices]RecipeGetByIdUseCase getByIdUseCase)
        {
            var response = await getByIdUseCase.Execute(recipeId);
            return Ok(response);
        }

        [HttpGet]
        [Route("getByUser/{numberOfRecipes:int}")]
        [ProducesResponseType(typeof(List<ResponseShortRecipeJson>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByUser([FromRoute]int numberOfRecipes, [FromServices]RecipeGetByUserUseCase getByUserUseCase)
        {
            var response = await getByUserUseCase.Execute(numberOfRecipes);
            return Ok(response);
        }

        [HttpDelete]
        [Route("deleteById/{recipeId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid recipeId, [FromServices] RecipeDeleteByIdUseCase deleteByIdUseCase)
        {
            await deleteByIdUseCase.Execute(recipeId);
            return NoContent();
        }

        [HttpPut]
        [Route("update/{recipeId:guid}")]
        [ProducesResponseType(typeof(ResponseRecipeJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update([FromBody] RequestRecipeJson newRecipe, [FromRoute] Guid recipeId, [FromServices]RecipeUpdateUseCase recipeUpdateUseCase)
        {
            var response = await recipeUpdateUseCase.Execute(recipeId, newRecipe);
            return Ok(response);
        }

        [HttpPost]
        [Route("generateWithAI")]
        [ProducesResponseType(typeof(ResponseRecipeJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateWithAI([FromBody]RequestRecipeIngredientsForAIJson ingredients, [FromServices]RecipeGenerateWithAIUseCase recipeGenerateWithAIUseCase)
        {
            var response = await recipeGenerateWithAIUseCase.Execute(ingredients);
            return Ok(response);
        }

        [HttpPut]
        [Route("/update/image/{recipeId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorJson), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImage(IFormFile file, [FromRoute]Guid recipeId, [FromServices]RecipeUpdateImageUseCase recipeUpdateImageUseCase)
        {
            await recipeUpdateImageUseCase.Execute(file, recipeId);
            return NoContent();
        }
    }
}

