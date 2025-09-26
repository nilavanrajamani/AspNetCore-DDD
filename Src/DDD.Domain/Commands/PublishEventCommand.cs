using System;

using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class PublishEventCommand : EventCommand
{
    public PublishEventCommand(Guid eventId)
    {
        Id = eventId;
        AggregateId = eventId;
    }

    public override bool IsValid()
    {
        ValidationResult = new PublishEventCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}