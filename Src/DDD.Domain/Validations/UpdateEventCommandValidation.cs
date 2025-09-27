using DDD.Domain.Commands;

namespace DDD.Domain.Validations;

public class UpdateEventCommandValidation : EventValidation<UpdateEventCommand>
{
    public UpdateEventCommandValidation()
    {
        ValidateId();
        ValidateTitle();
        ValidateDescription();
        ValidateVenueId();
        ValidateStartDate();
        ValidateEndDate();
    }
}