using System;

using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class EventUnpublishedEvent : Event
{
    public EventUnpublishedEvent(Guid eventId, Guid organizerId, string reason)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        Reason = reason;
        AggregateId = eventId;
    }

    public Guid EventId { get; set; }

    public Guid OrganizerId { get; private set; }

    public string Reason { get; private set; }
}