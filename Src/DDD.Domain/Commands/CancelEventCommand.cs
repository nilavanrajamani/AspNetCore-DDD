using System;

using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class CancelEventCommand : EventCommand
{
    public CancelEventCommand(Guid eventId, string reason, bool initiateRefunds = true)
    {
        Id = eventId;
        Reason = reason;
        InitiateRefunds = initiateRefunds;
        AggregateId = eventId;
    }

    public string Reason { get; private set; }

    public bool InitiateRefunds { get; private set; }

    public override bool IsValid()
    {
        ValidationResult = new CancelEventCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}