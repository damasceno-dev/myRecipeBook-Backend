using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Email;

namespace MyRecipeBook.Application.UseCases.Users.ResetPassword;

public class UserGetResetPasswordCodeUseCase (IUsersRepository usersRepository, IUnitOfWork unitOfWork, ISendUserResetPasswordCode sendUserResetPasswordCode)
{
    public async Task Execute(string email)
    {
        var user = await usersRepository.GetExistingUserWithEmail(email);
        if (user is not null)
        {
            await usersRepository.DeactivateExistingResetPasswordCodes(user.Id);
            
            var codeRandom = Generate6DigitCode();
            var codeToResetPassword = new UserPasswordResetCode { UserId = user.Id, Code = codeRandom };
            await usersRepository.AddResetPasswordCode(codeToResetPassword);
            
            await unitOfWork.Commit();
            
            await sendUserResetPasswordCode.Send(user.Email, codeRandom);
        }
    }

    private static string Generate6DigitCode()
    {
        var random = new Random();
        return random.Next(100000, 1000000).ToString();
    }
}