using System;

using DDD.Domain.Core.Events;

namespace DDD.Domain.Events;

public class EventCreatedEvent : Event
{
    public EventCreatedEvent(Guid id, string title, string description, Guid organizerId, Guid venueId, DateTime startDate, DateTime endDate)
    {
        Id = id;
        Title = title;
        Description = description;
        OrganizerId = organizerId;
        VenueId = venueId;
        StartDate = startDate;
        EndDate = endDate;
        AggregateId = id;
    }

    public Guid Id { get; set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public Guid OrganizerId { get; private set; }

    public Guid VenueId { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }
}