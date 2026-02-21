using AutoMapper;
using Weather.Current.Application.DTOs;
using Weather.Current.Domain.Entities;

namespace Weather.Current.Application.Mappings;

public class WeatherMappingProfile : Profile
{
    public WeatherMappingProfile()
    {
        // Domain entity → DTO
        CreateMap<WeatherResponse, WeatherResponseDto>()
            .ForMember(dest => dest.City,                    opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.Country,                 opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.AdminRegion,             opt => opt.MapFrom(src => src.Region))
            .ForMember(dest => dest.Latitude,                opt => opt.MapFrom(src => src.Latitude))
            .ForMember(dest => dest.Longitude,               opt => opt.MapFrom(src => src.Longitude))
            .ForMember(dest => dest.CurrentTime,             opt => opt.MapFrom(src => src.LocalTime))
            .ForMember(dest => dest.CurrentTemperature,      opt => opt.MapFrom(src => src.TemperatureC))
            .ForMember(dest => dest.CurrentWindSpeed,        opt => opt.MapFrom(src => src.WindKph))
            .ForMember(dest => dest.CurrentRelativeHumidity, opt => opt.MapFrom(src => src.Humidity));

        // DTO → Domain entity
        CreateMap<WeatherRequestDto, WeatherRequest>()
            .ForMember(dest => dest.City,        opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode))
            .ForMember(dest => dest.RequestedAt, opt => opt.Ignore());
    }
}
