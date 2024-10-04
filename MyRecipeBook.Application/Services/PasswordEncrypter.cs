namespace MyRecipeBook.Application.Services;

public static class PasswordEncrypter
{
    private const string ChaveAdicional = "recipeKey";
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword($"{ChaveAdicional}{password}");
    }
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify($"{ChaveAdicional}{password}", hashedPassword);
    }
}