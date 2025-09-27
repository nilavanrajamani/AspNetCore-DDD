using System;

using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public class SetEventVisibilityCommandValidation : AbstractValidator<SetEventVisibilityCommand>
{
    public SetEventVisibilityCommandValidation()
    {
        ValidateEventId();
        ValidateVisibility();
    }

    protected void ValidateEventId()
    {
        RuleFor(c => c.EventId)
            .NotEqual(Guid.Empty)
            .WithMessage("Event ID is required");
    }

    protected void ValidateVisibility()
    {
        RuleFor(c => c.Visibility)
            .IsInEnum()
            .WithMessage("Valid visibility value is required (Private, Public, or InviteOnly)");
    }
}