using DDD.Domain.Core.Events;
using DDD.Domain.Models;

namespace DDD.Domain.Events;

public class UserInvitedToEventEvent : Core.Events.Event
{
    public UserInvitedToEventEvent(System.Guid eventId, System.Guid organizerId, System.Guid userId, InvitationRole role)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        UserId = userId;
        Role = role;
        AggregateId = eventId;
    }

    public System.Guid EventId { get; private set; }

    public System.Guid OrganizerId { get; private set; }

    public System.Guid UserId { get; private set; }

    public InvitationRole Role { get; private set; }
}