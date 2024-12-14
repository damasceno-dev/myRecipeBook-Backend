using Microsoft.AspNetCore.Http;

namespace MyRecipeBook.Communication.Requests;

public class RequestRecipeForm : RequestRecipeJson
{
    public IFormFile? ImageFile { get; set; }
}