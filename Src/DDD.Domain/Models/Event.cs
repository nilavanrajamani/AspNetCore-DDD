using System;
using DDD.Domain.Core.Models;

namespace DDD.Domain.Models;

public class Event : EntityAudit
{
    public Event(Guid id, string title, string description, Guid organizerId, Guid venueId, DateTime startDate, DateTime endDate)
    {
        Id = id;
        Title = title;
        Description = description;
        OrganizerId = organizerId;
        VenueId = venueId;
        StartDate = startDate;
        EndDate = endDate;
        Status = EventStatus.Draft;
        Visibility = EventVisibility.Private;
        CreatedAt = DateTime.UtcNow;
    }

    // Empty constructor for EF
    protected Event()
    {
    }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public Guid OrganizerId { get; private set; }

    public Guid VenueId { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public EventStatus Status { get; private set; }

    public EventVisibility Visibility { get; private set; }

    public void UpdateDetails(string title, string description, DateTime startDate, DateTime endDate)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published");

        Status = EventStatus.Published;
        Visibility = EventVisibility.Public;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Event is already cancelled");

        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed event");

        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum EventStatus
{
    Draft,
    Published,
    Cancelled,
    Completed
}

public enum EventVisibility
{
    Private,
    Public,
    InviteOnly
}