using Bogus.DataSets;
using CommonTestUtilities.Mapper;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Recipes.Register;
using MyRecipeBook.Application.UseCases.Users.Register;
using MyRecipeBook.Domain.Entities;
using Xunit;

namespace UseCases.Test.UseCases.Recipes.Register;

public class RecipeRegisterUseCaseTest
{
    
    [Fact]
    public async Task Success()
    {
        var request = RequestRecipeRegisterJsonBuilder.Build();
        var useCase = CreateRecipeRegisterUseCase();
        var response = await useCase.Execute(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Title.Should().Be(request.Title);
    }

    private static RecipeRegisterUseCase CreateRecipeRegisterUseCase()
    {
        var mapper = MapperBuilder.Build();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var usersRepository = new UserRepositoryBuilder().GetLoggedUserWithToken(new User {Name = "test_user", Email = "test_user@test.com"}).Build();
        var recipeRepository = new RecipeRepositoryBuilder().Build();
        return new RecipeRegisterUseCase(mapper, unitOfWork, usersRepository, recipeRepository);
    }
}