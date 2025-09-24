using System;

using DDD.Domain.Core.Commands;

namespace DDD.Domain.Commands;

public abstract class EventCommand : Command
{
    public Guid Id { get; protected set; }

    public string Title { get; protected set; }

    public string Description { get; protected set; }

    public Guid OrganizerId { get; protected set; }

    public Guid VenueId { get; protected set; }

    public DateTime StartDate { get; protected set; }

    public DateTime EndDate { get; protected set; }
}