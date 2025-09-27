using DDD.Domain.Commands;

using FluentValidation;

namespace DDD.Domain.Validations;

public class RemoveUserInvitationCommandValidation : AbstractValidator<RemoveUserInvitationCommand>
{
    public RemoveUserInvitationCommandValidation()
    {
        ValidateEventId();
        ValidateUserId();
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
}