using System;
using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class CreateEventCommand : EventCommand
{
    public CreateEventCommand(string title, string description, Guid organizerId, Guid venueId, DateTime startDate, DateTime endDate)
    {
        Title = title;
        Description = description;
        OrganizerId = organizerId;
        VenueId = venueId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public override bool IsValid()
    {
        ValidationResult = new CreateEventCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}