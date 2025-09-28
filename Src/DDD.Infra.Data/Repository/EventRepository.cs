using System;
using System.Collections.Generic;
using System.Linq;

using DDD.Domain.Interfaces;
using DDD.Domain.Models;
using DDD.Domain.Specifications;
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

    public IEnumerable<Event> GetPublishedEvents()
    {
        return _dbSet
            .Include(e => e.PricingTiers)
            .Where(e => e.Status == EventStatus.Published)
            .ToList();
    }

    public IEnumerable<Event> GetEventsByStatus(EventStatus status)
    {
        return _dbSet
            .Include(e => e.PricingTiers)
            .Where(e => e.Status == status)
            .ToList();
    }

    public IEnumerable<Event> GetEventsByOrganizer(Guid organizerId)
    {
        return _dbSet
            .Include(e => e.PricingTiers)
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }

    public IEnumerable<Event> GetEventsByOrganizerAndStatus(Guid organizerId, EventStatus status)
    {
        return _dbSet
            .Include(e => e.PricingTiers)
            .Where(e => e.OrganizerId == organizerId && e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }
    
    /// <summary>
    /// Gets events using specification pattern for advanced filtering.
    /// This supports the "filter my events only" requirement and other complex queries.
    /// </summary>
    /// <param name="specification">The specification containing the filtering criteria.</param>
    /// <returns>A queryable collection of events matching the specification.</returns>
    public IQueryable<Event> GetEventsWithSpecification(ISpecification<Event> specification)
    {
        return GetAll(specification);
    }
    
    /// <summary>
    /// Gets "my events only" filtered by organizer ID using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <returns>A queryable collection of events organized by the specified user.</returns>
    public IQueryable<Event> GetMyEventsOnly(Guid organizerId)
    {
        var spec = new MyEventsOnlySpecification(organizerId);
        return GetAll(spec);
    }
    
    /// <summary>
    /// Gets "my events only" with status filtering using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="status">The event status to filter by.</param>
    /// <returns>A queryable collection of events organized by the specified user with the given status.</returns>
    public IQueryable<Event> GetMyEventsOnly(Guid organizerId, EventStatus status)
    {
        var spec = new MyEventsOnlySpecification(organizerId, status);
        return GetAll(spec);
    }
    
    /// <summary>
    /// Gets "my events only" with pagination using specification pattern.
    /// </summary>
    /// <param name="organizerId">The ID of the organizer whose events to retrieve.</param>
    /// <param name="skip">The number of events to skip for pagination.</param>
    /// <param name="take">The number of events to take for pagination.</param>
    /// <returns>A queryable collection of events organized by the specified user with pagination applied.</returns>
    public IQueryable<Event> GetMyEventsOnly(Guid organizerId, int skip, int take)
    {
        var spec = new MyEventsOnlySpecification(organizerId, skip: skip, take: take);
        return GetAll(spec);
    }
}