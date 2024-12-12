using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class JsonModelBinderAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BindingSource.Custom;
}