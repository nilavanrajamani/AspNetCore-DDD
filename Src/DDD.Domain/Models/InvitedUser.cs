using DDD.Domain.Core.Models;

namespace DDD.Domain.Models;

public class InvitedUser : EntityAudit
{
    public InvitedUser(Guid id, Guid eventId, Guid userId, InvitationRole role)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    // Empty constructor for EF
    protected InvitedUser()
    {
    }

    public Guid EventId { get; private set; }

    public Guid UserId { get; private set; }

    public InvitationRole Role { get; private set; }
}

public enum InvitationRole
{
    Attendee,
    Speaker,
    Sponsor,
    VIP,
}