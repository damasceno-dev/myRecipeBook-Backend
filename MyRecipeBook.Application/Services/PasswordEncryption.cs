namespace MyRecipeBook.Application.Services;

public class PasswordEncryption(string additionalKey, string defaultExternalLoginKey)
{
    public string GetDefaultExternalLoginKey()
    {
        return defaultExternalLoginKey;
    }
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword($"{additionalKey}{password}");
    }
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify($"{additionalKey}{password}", hashedPassword);
    }
}