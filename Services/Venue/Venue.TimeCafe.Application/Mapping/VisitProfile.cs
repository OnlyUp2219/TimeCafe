namespace Venue.TimeCafe.Application.Mapping;

public class VisitProfile : Profile
{
    public VisitProfile()
    {
        CreateMap<Visit, VisitWithTariffDto>().ReverseMap();

        CreateMap<UpdateVisitCommand, VisitWithTariffDto>()
            .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => Guid.Parse(src.VisitId)))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)))
            .ForMember(dest => dest.TariffId, opt => opt.MapFrom(src => Guid.Parse(src.TariffId)))
            .ForMember(dest => dest.EntryTime, opt => opt.MapFrom(src => src.EntryTime))
            .ForMember(dest => dest.ExitTime, opt => opt.MapFrom(src => src.ExitTime))
            .ForMember(dest => dest.CalculatedCost, opt => opt.MapFrom(src => src.CalculatedCost))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<UpdateVisitCommand, Visit>()
            .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => Guid.Parse(src.VisitId)))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)))
            .ForMember(dest => dest.TariffId, opt => opt.MapFrom(src => Guid.Parse(src.TariffId)))
            .ForMember(dest => dest.EntryTime, opt => opt.MapFrom(src => src.EntryTime))
            .ForMember(dest => dest.ExitTime, opt => opt.MapFrom(src => src.ExitTime))
            .ForMember(dest => dest.CalculatedCost, opt => opt.MapFrom(src => src.CalculatedCost))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
    }
}