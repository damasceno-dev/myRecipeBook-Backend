using AutoMapper;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Application.Services;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        RequestToDomain();
        DomainToResponse();
    }

    private void DomainToResponse()
    {
        CreateMap<User, ResponseUserRegisterJson>();
    }

    private void RequestToDomain()
    {
        CreateMap<RequestUserRegisterJson, User>()
            .ForMember(u => u.Password, config => config.Ignore());
    }
}