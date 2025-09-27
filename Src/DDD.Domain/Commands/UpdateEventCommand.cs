using System;

using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class UpdateEventCommand : EventCommand
{
    public bool ForceUpdate { get; set; }

    public UpdateEventCommand(Guid id, string title, string description, Guid venueId, DateTime startDate, DateTime endDate, bool forceUpdate = false)
    {
        Id = id;
        Title = title;
        Description = description;
        VenueId = venueId;
        StartDate = startDate;
        EndDate = endDate;
        ForceUpdate = forceUpdate;
    }

    public override bool IsValid()
    {
        ValidationResult = new UpdateEventCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}