using System;

using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public class InviteUserToEventCommandValidation : AbstractValidator<InviteUserToEventCommand>
{
    public InviteUserToEventCommandValidation()
    {
        ValidateEventId();
        ValidateUserId();
        ValidateRole();
    }

    protected void ValidateEventId()
    {
        RuleFor(c => c.EventId)
            .NotEqual(Guid.Empty)
            .WithMessage("Event ID is required");
    }

    protected void ValidateUserId()
    {
        RuleFor(c => c.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");
    }

    protected void ValidateRole()
    {
        RuleFor(c => c.Role)
            .IsInEnum()
            .WithMessage("Valid invitation role is required (Attendee, Speaker, Sponsor, or VIP)");
    }
}