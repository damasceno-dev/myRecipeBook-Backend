using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Application.UseCases.Users.Delete;

public class UserQueueDeletionUseCase(IUsersRepository usersRepository, IUnitOfWork unitOfWork, IDeleteUserQueue deleteUserQueue)
{
    public async Task Execute()
    {
        var user = await usersRepository.GetLoggedUserWithToken();
        user.Active = false;
        usersRepository.UpdateUser(user);
        
        await unitOfWork.Commit();
        await deleteUserQueue.SendMessageAsync(user.Id);
    }
}