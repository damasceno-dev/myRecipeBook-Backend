namespace MyRecipeBook.Application.Services;

public class PasswordEncryption
{
    private readonly string _additionalKey;
    public static string DefaultExternalLoginKey;
    public PasswordEncryption(string additionalKey, string defaultExternalLoginKey)
    {
        _additionalKey = additionalKey;
        DefaultExternalLoginKey = defaultExternalLoginKey;
    }
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword($"{_additionalKey}{password}");
    }
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify($"{_additionalKey}{password}", hashedPassword);
    }
}