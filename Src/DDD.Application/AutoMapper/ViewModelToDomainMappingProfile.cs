using AutoMapper;

using DDD.Application.ViewModels;
using DDD.Domain.Commands;

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
    }
}
