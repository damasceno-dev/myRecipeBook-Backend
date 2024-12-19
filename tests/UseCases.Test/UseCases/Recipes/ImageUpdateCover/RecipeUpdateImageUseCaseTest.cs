using CommonTestUtilities.Entities;
using CommonTestUtilities.FormFile;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Services;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.ImageUpdateCover;
using MyRecipeBook.Communication;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Exception;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.ImageUpdateCover;

public class RecipeUpdateImageUseCaseTest
{
    [Theory]
    [InlineData("jpgImage.jpg")]
    [InlineData("pngImage.png")]
    public async Task SuccessWithImage(string imageName)
    {
        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
        var useCase = CreateRecipeUpdateImageUseCase(user, recipe);
        var file = FormFileBuilder.Build(imageName);
        var extension = Path.GetExtension(imageName);
        
        var act = async () => await useCase.Execute(file, recipe.Id);
        
        await act.Should().NotThrowAsync();
        recipe.ImageIdentifier.Should().NotBeNull();
        recipe.ImageIdentifier.Should().EndWith(extension);
    }
    
    [Fact]
    public async Task ErrorInvalidFileType()
    {
        var (user, _) = UserBuilder.Build();
        var recipe = RecipeBuilder.Build(user);
        var useCase = CreateRecipeUpdateImageUseCase(user, recipe);
        var file = FormFileBuilder.Build("textFile.txt");
        
        var act = async () => await useCase.Execute(file, recipe.Id);
        
        var exception = await act.Should().ThrowAsync<OnValidationException>();
        exception.And.GetErrors.Should().ContainSingle(ResourceErrorMessages.IMAGE_INVALID_TYPE);
    }

    private static RecipeUpdateImageUseCase CreateRecipeUpdateImageUseCase(User user, Recipe recipe)
    {
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(user).Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var recipeRepository = new RecipeRepositoryBuilder().GetById(recipe).Build();
        var storageService = new StorageServiceBuilder().Upload().Build();
        
        return new RecipeUpdateImageUseCase(storageService, recipeRepository, usersRepository, unitOfWork);
    }
}