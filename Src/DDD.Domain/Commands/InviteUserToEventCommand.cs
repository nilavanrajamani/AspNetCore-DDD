using System;

using DDD.Domain.Core.Commands;
using DDD.Domain.Models;
using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class InviteUserToEventCommand : Command
{
    public InviteUserToEventCommand(Guid eventId, Guid userId, InvitationRole role = InvitationRole.Attendee)
    {
        EventId = eventId;
        UserId = userId;
        Role = role;
    }

    public Guid EventId { get; set; }

    public Guid UserId { get; set; }

    public InvitationRole Role { get; set; }

    public override bool IsValid()
    {
        ValidationResult = new InviteUserToEventCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}