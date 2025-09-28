using System;
using System.Collections.Generic;
using System.Linq;

using DDD.Domain.Models;

namespace DDD.Domain.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Event GetByTitle(string title);

    Event GetByOrganizerAndTitle(Guid organizerId, string title);
    
    void UpdateEventPricingTiers(Event eventEntity, IEnumerable<PricingTierDefinition> pricingTiers);

    IEnumerable<Event> GetPublishedEvents();

    IEnumerable<Event> GetEventsByStatus(EventStatus status);

    IEnumerable<Event> GetEventsByOrganizer(Guid organizerId);

    IEnumerable<Event> GetEventsByOrganizerAndStatus(Guid organizerId, EventStatus status);
    
    /// <summary>
    /// Gets events using specification pattern for advanced filtering.
    /// This supports the "filter my events only" requirement and other complex queries.
    /// </summary>
    /// <param name="specification">The specification containing the filtering criteria.</param>
    /// <returns>A queryable collection of events matching the specification.</returns>
    IQueryable<Event> GetEventsWithSpecification(ISpecification<Event> specification);
    
    /// <summary>
    /// Gets "my events only" filtered by organizer ID using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <returns>A queryable collection of events organized by the specified user.</returns>
    IQueryable<Event> GetMyEventsOnly(Guid organizerId);
    
    /// <summary>
    /// Gets "my events only" with status filtering using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="status">The event status to filter by.</param>
    /// <returns>A queryable collection of events organized by the specified user with the given status.</returns>
    IQueryable<Event> GetMyEventsOnly(Guid organizerId, EventStatus status);
    
    /// <summary>
    /// Gets "my events only" with pagination using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="skip">The number of events to skip for pagination.</param>
    /// <param name="take">The number of events to take for pagination.</param>
    /// <returns>A queryable collection of events organized by the specified user with pagination applied.</returns>
    IQueryable<Event> GetMyEventsOnly(Guid organizerId, int skip, int take);
}