using Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

namespace Venue.TimeCafe.Application.Mapping;

public class TariffProfile : Profile
{
    public TariffProfile()
    {
        CreateMap<Tariff, TariffWithThemeDto>().ReverseMap();
        //TODO : null
        CreateMap<UpdateTariffCommand, TariffWithThemeDto>()
            .ForMember(dest => dest.TariffId, opt => opt.MapFrom(src => Guid.Parse(src.TariffId)))
            .ForMember(dest => dest.ThemeId, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ThemeId) ? (Guid?)null : Guid.Parse(src.ThemeId)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.PricePerMinute, opt => opt.MapFrom(src => src.PricePerMinute))
            .ForMember(dest => dest.BillingType, opt => opt.MapFrom(src => src.BillingType))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<UpdateTariffCommand, Tariff>()
            .ForMember(dest => dest.TariffId, opt => opt.MapFrom(src => Guid.Parse(src.TariffId)))
            .ForMember(dest => dest.ThemeId, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.ThemeId) ? (Guid?)null : Guid.Parse(src.ThemeId)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.PricePerMinute, opt => opt.MapFrom(src => src.PricePerMinute))
            .ForMember(dest => dest.BillingType, opt => opt.MapFrom(src => src.BillingType))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}