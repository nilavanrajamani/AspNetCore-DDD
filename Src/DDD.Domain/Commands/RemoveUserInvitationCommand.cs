using DDD.Domain.Core.Commands;
using DDD.Domain.Validations;

namespace DDD.Domain.Commands;

public class RemoveUserInvitationCommand : Command
{
    public RemoveUserInvitationCommand(System.Guid eventId, System.Guid userId)
    {
        EventId = eventId;
        UserId = userId;
    }

    public System.Guid EventId { get; set; }

    public System.Guid UserId { get; set; }

    public override bool IsValid()
    {
        ValidationResult = new RemoveUserInvitationCommandValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}