using System;

using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class EventPublishedEvent : Event
{
    public EventPublishedEvent(Guid eventId, Guid organizerId, string eventTitle, DateTime startDate, int totalCapacity)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        EventTitle = eventTitle;
        StartDate = startDate;
        TotalCapacity = totalCapacity;
        AggregateId = eventId;
    }

    public Guid EventId { get; set; }

    public Guid OrganizerId { get; private set; }

    public string EventTitle { get; private set; }

    public DateTime StartDate { get; private set; }

    public int TotalCapacity { get; private set; }
}