using System;

using DDD.Domain.Core.Events;
using DDD.Domain.Models;

namespace DDD.Domain.Events;

public class EventCancelledEvent : Core.Events.Event
{
    public Guid EventId { get; set; }

    public Guid OrganizerId { get; set; }

    public string Reason { get; set; }

    public EventStatus PreviousStatus { get; set; }

    public bool InitiateRefunds { get; set; }

    public EventCancelledEvent(Guid eventId, Guid organizerId, string reason, EventStatus previousStatus, bool initiateRefunds)
    {
        EventId = eventId;
        OrganizerId = organizerId;
        Reason = reason;
        PreviousStatus = previousStatus;
        InitiateRefunds = initiateRefunds;
        AggregateId = eventId;
    }
}