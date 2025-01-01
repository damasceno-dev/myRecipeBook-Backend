namespace MyRecipeBook.Application.Services;

public abstract class DigitGenerator
{
    public static string Generate6DigitCode()
    {
        return new Random().Next(100000, 1000000).ToString();
    }
}