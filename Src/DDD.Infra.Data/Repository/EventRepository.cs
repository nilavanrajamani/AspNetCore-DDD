using System;
using System.Collections.Generic;
using System.Linq;

using DDD.Domain.Interfaces;
using DDD.Domain.Models;
using DDD.Infra.Data.Context;

using Microsoft.EntityFrameworkCore;

namespace DDD.Infra.Data.Repository;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public override Event GetById(Guid id)
    {
        return _dbSet
            .Include(e => e.PricingTiers)
            .FirstOrDefault(e => e.Id == id);
    }

    public void UpdateEventPricingTiers(Event eventEntity, IEnumerable<PricingTierDefinition> pricingTiers)
    {
        // Remove existing pricing tiers explicitly
        var existingTiers = _db.Set<PricingTier>().Where(pt => pt.EventId == eventEntity.Id).ToList();
        _db.Set<PricingTier>().RemoveRange(existingTiers);
        
        // Add new pricing tiers
        foreach (var tierDef in pricingTiers)
        {
            var newTier = new PricingTier(
                Guid.NewGuid(),
                eventEntity.Id,
                tierDef.Name,
                tierDef.Price,
                tierDef.Currency,
                tierDef.Capacity,
                tierDef.SaleStartDate,
                tierDef.SaleEndDate);
                
            _db.Set<PricingTier>().Add(newTier);
        }
    }

    public Event GetByTitle(string title)
    {
        return _dbSet.AsNoTracking().FirstOrDefault(e => e.Title == title);
    }

    public Event GetByOrganizerAndTitle(Guid organizerId, string title)
    {
        return _dbSet.AsNoTracking()
            .FirstOrDefault(e => e.OrganizerId == organizerId && e.Title == title);
    }
}