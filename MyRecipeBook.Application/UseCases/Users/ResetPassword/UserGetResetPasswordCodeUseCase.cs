using MyRecipeBook.Application.Services;
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

            var codeRandom = DigitGenerator.Generate6DigitCode();
            var codeToResetPassword = new UserPasswordResetCode { UserId = user.Id, Code = codeRandom };
            await usersRepository.AddResetPasswordCode(codeToResetPassword);
            
            await unitOfWork.Commit();
            
            await sendUserResetPasswordCode.Send(user.Email, codeRandom);
        }
    }
}