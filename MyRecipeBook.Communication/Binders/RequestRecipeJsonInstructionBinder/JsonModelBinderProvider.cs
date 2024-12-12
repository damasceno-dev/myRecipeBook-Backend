using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;

public class JsonModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata is { } metadata &&
            metadata.BinderType == typeof(JsonModelBinder))
        {
            return new JsonModelBinder();
        }

        return null;
    }
}