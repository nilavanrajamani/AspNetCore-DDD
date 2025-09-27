using System;

using DDD.Domain.Core.Commands;
using DDD.Domain.Models;
using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class SetEventVisibilityCommand : Command
{
    public SetEventVisibilityCommand(Guid eventId, EventVisibility visibility)
    {
        EventId = eventId;
        Visibility = visibility;
    }

    public Guid EventId { get; set; }

    public EventVisibility Visibility { get; set; }

    public override bool IsValid()
    {
        ValidationResult = new SetEventVisibilityCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}