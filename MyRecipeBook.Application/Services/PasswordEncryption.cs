namespace MyRecipeBook.Application.Services;

public class PasswordEncryption
{
    private readonly string _additionalKey;

    public PasswordEncryption(string additionalKey)
    {
        _additionalKey = additionalKey;
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