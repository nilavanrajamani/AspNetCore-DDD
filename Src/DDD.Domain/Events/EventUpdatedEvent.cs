using System;
using System.Collections.Generic;

using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class EventUpdatedEvent : Event
{
    public Guid EventId { get; set; }
    public Guid OrganizerId { get; set; }
    public List<EventChange> Changes { get; set; }

    public EventUpdatedEvent(Guid eventId, Guid organizerId, List<EventChange> changes)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        Changes = changes;
        MessageType = "EventUpdatedEvent";
        AggregateId = eventId;
    }
}

public class EventChange
{
    public string Field { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }

    public EventChange(string field, string oldValue, string newValue)
    {
        Field = field;
        OldValue = oldValue;
        NewValue = newValue;
    }
}