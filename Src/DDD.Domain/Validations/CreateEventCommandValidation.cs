using DDD.Domain.Commands;

namespace DDD.Domain.Validations;

public class CreateEventCommandValidation : EventValidation<CreateEventCommand>
{
    public CreateEventCommandValidation()
    {
        ValidateTitle();
        ValidateDescription();
        ValidateOrganizerId();
        ValidateVenueId();
        ValidateStartDate();
        ValidateEndDate();
    }
}