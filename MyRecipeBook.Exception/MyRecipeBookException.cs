namespace MyRecipeBook.Exception;

public abstract class MyRecipeBookException : SystemException
{
    public MyRecipeBookException(string message) : base (message) { }
    public abstract int GetStatusCode { get; }
    public abstract List<string> GetErrors { get; }
}