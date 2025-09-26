using System;

using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class UnpublishEventCommand : EventCommand
{
    public UnpublishEventCommand(Guid eventId, string reason)
    {
        Id = eventId;
        Reason = reason;
        AggregateId = eventId;
    }

    public string Reason { get; private set; }

    public override bool IsValid()
    {
        ValidationResult = new UnpublishEventCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}