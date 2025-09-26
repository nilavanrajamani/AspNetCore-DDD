using System;

using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class EventCapacitySetEvent : Event
{
    public EventCapacitySetEvent(Guid eventId, int totalCapacity, int pricingTiersCount)
    {
        EventId = eventId;
        TotalCapacity = totalCapacity;
        PricingTiersCount = pricingTiersCount;
        AggregateId = eventId;
    }

    public Guid EventId { get; set; }

    public int TotalCapacity { get; private set; }

    public int PricingTiersCount { get; private set; }
}