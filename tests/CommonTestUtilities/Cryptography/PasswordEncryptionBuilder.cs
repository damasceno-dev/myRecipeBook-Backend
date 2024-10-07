using MyRecipeBook.Application.Services;

namespace CommonTestUtilities.Cryptography;

public static class PasswordEncryptionBuilder
{
    public static PasswordEncryption Build()
    {
        return new PasswordEncryption("testKey1234");
    }
}