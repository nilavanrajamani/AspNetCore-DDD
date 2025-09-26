using System.Linq;

using AutoMapper;

using DDD.Application.ViewModels;
using DDD.Domain.Commands;
using DDD.Domain.Models;

namespace DDD.Application.AutoMapper;

public class ViewModelToDomainMappingProfile : Profile
{
    public ViewModelToDomainMappingProfile()
    {
        CreateMap<CustomerViewModel, RegisterNewCustomerCommand>()
            .ConstructUsing(c => new RegisterNewCustomerCommand(c.Name, c.Email, c.BirthDate));
        CreateMap<CustomerViewModel, UpdateCustomerCommand>()
            .ConstructUsing(c => new UpdateCustomerCommand(c.Id, c.Name, c.Email, c.BirthDate));

        CreateMap<EventViewModel, CreateEventCommand>()
            .ConstructUsing(e => new CreateEventCommand(e.Title, e.Description, e.OrganizerId, e.VenueId, e.StartDate, e.EndDate));

        CreateMap<SetEventCapacityViewModel, SetEventCapacityCommand>()
            .ConstructUsing(c => new SetEventCapacityCommand(
                c.EventId,
                c.TotalCapacity,
                c.PricingTiers.Select(t => new PricingTierDefinition(
                    t.Name,
                    t.Price,
                    t.Currency,
                    t.Capacity,
                    t.SaleStartDate,
                    t.SaleEndDate)).ToList()));

        CreateMap<PricingTierViewModel, PricingTierDefinition>()
            .ConstructUsing(t => new PricingTierDefinition(
                t.Name,
                t.Price,
                t.Currency,
                t.Capacity,
                t.SaleStartDate,
                t.SaleEndDate));
    }
}
