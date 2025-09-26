using AutoMapper;

using DDD.Application.ViewModels;
using DDD.Domain.Models;

namespace DDD.Application.AutoMapper;

public class DomainToViewModelMappingProfile : Profile
{
    public DomainToViewModelMappingProfile()
    {
        CreateMap<Customer, CustomerViewModel>();
        CreateMap<Event, EventViewModel>();
        CreateMap<Venue, VenueViewModel>();
        CreateMap<PricingTier, PricingTierViewModel>();
        CreateMap<Event, SetEventCapacityViewModel>()
            .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TotalCapacity, opt => opt.MapFrom(src => src.TotalCapacity ?? 0));
    }
}
