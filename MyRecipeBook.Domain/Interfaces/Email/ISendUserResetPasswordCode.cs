namespace MyRecipeBook.Domain.Interfaces.Email;

public interface ISendUserResetPasswordCode
{
    Task Send(string userEmail, string code);
}