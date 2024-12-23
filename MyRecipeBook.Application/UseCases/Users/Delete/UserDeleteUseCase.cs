using MyRecipeBook.Domain.Interfaces;

namespace MyRecipeBook.Application.UseCases.Users.Delete;

public class UserDeleteUseCase(IUsersRepository usersRepository, IStorageService storageService, IUnitOfWork unitOfWork)
{
    public async Task Execute(Guid userId)
    {
        await storageService.DeleteContainer(userId);

        await usersRepository.DeleteAccount(userId);

        await unitOfWork.Commit();
    }
}