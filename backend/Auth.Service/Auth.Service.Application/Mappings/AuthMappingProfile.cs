using Auth.Service.Application.DTOs;
using Auth.Service.Domain.Entities;
using AutoMapper;

namespace Auth.Service.Application.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // Entity → DTO
        CreateMap<ApplicationUser, AuthResponse>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.Ignore());

        // DTO → Entity
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username));
    }
}
