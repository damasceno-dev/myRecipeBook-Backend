using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Interfaces;
using MyRecipeBook.Domain.Interfaces.Tokens;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Users.ExternalLogin;

public class UserExternalLoginUseCase(IUsersRepository usersRepository, IUnitOfWork unitOfWork, ITokenRepository tokenRepository)
{
    public async Task<string> Execute(string name, string email)
    {
        var user = await usersRepository.GetExistingUserWithEmail(email);
        
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Password = "-"
            };

            await usersRepository.Register(user);
            await unitOfWork.Commit();
        }
        
        if (user.Active is false)
        {
            throw new InvalidLoginException(ResourceErrorMessages.EMAIL_NOT_ACTIVE);
        }

        return tokenRepository.Generate(user.Id);
    }
}