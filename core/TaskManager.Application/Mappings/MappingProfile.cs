using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.DTOs.RefreshToken;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Core.Entities;

namespace TaskManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //User
        CreateMap<User, UserResponseDto>();
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcProperty) => srcProperty != null));
        //Task
        CreateMap<ManagedTask, ManagedTaskResponseDto>();
        CreateMap<CreateManagedTaskDto, ManagedTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<UpdateManagedTaskDto, ManagedTask>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        //Tag
        CreateMap<Tag, TagResponseDto>();
        CreateMap<UpdateTagDto, Tag>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<CreateTagDto, Tag>();

        //Tokens
        CreateMap<RefreshToken, RefreshTokenDto>();
        CreateMap<RefreshTokenDto, RefreshToken>();
    }
}
