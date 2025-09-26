using System;
using System.Collections.Generic;

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
}