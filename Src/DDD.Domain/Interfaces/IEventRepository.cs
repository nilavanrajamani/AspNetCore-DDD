using System;
using System.Collections.Generic;

using DDD.Domain.Models;

namespace DDD.Domain.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Event GetByTitle(string title);

    Event GetByOrganizerAndTitle(Guid organizerId, string title);
    
    void UpdateEventPricingTiers(Event eventEntity, IEnumerable<PricingTierDefinition> pricingTiers);
}