using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class EventVisibilityChangedEvent : Event
{
    public EventVisibilityChangedEvent(System.Guid eventId, System.Guid organizerId, string previousVisibility, string newVisibility)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        PreviousVisibility = previousVisibility;
        NewVisibility = newVisibility;
        AggregateId = eventId;
    }

    public System.Guid EventId { get; private set; }

    public System.Guid OrganizerId { get; private set; }

    public string PreviousVisibility { get; private set; }

    public string NewVisibility { get; private set; }
}