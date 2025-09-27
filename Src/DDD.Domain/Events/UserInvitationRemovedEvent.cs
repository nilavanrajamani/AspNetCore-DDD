using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class UserInvitationRemovedEvent : Event
{
    public UserInvitationRemovedEvent(System.Guid eventId, System.Guid organizerId, System.Guid userId)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        UserId = userId;
        AggregateId = eventId;
    }

    public System.Guid EventId { get; private set; }

    public System.Guid OrganizerId { get; private set; }

    public System.Guid UserId { get; private set; }
}