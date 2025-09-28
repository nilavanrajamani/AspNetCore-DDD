using System;
using System.Linq.Expressions;

using DDD.Domain.Models;

namespace DDD.Domain.Specifications;

/// <summary>
/// Specification to filter events by organizer (my events only).
/// This implements the specification pattern to encapsulate the business logic
/// for filtering events that belong to a specific organizer/user.
/// </summary>
public class EventsByOrganizerSpecification : BaseSpecification<Event>
{
    public EventsByOrganizerSpecification(Guid organizerId)
        : base(eventEntity => eventEntity.OrganizerId == organizerId)
    {
        // Include pricing tiers by default for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users for invite-only events
        AddInclude(e => e.InvitedUsers);
        
        // Order by creation date descending (newest first)
        ApplyOrderByDescending(e => e.CreatedAt);
    }
    
    /// <summary>
    /// Constructor that allows additional filtering by event status.
    /// </summary>
    public EventsByOrganizerSpecification(Guid organizerId, EventStatus status)
        : base(eventEntity => eventEntity.OrganizerId == organizerId && eventEntity.Status == status)
    {
        // Include pricing tiers by default for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users for invite-only events
        AddInclude(e => e.InvitedUsers);
        
        // Order by creation date descending (newest first)
        ApplyOrderByDescending(e => e.CreatedAt);
    }
    
    /// <summary>
    /// Constructor with pagination support for "my events only" filtering.
    /// </summary>
    public EventsByOrganizerSpecification(Guid organizerId, int skip, int take)
        : base(eventEntity => eventEntity.OrganizerId == organizerId)
    {
        // Include pricing tiers by default for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users for invite-only events
        AddInclude(e => e.InvitedUsers);
        
        // Order by creation date descending (newest first)
        ApplyOrderByDescending(e => e.CreatedAt);
        
        // Apply pagination
        ApplyPaging(skip, take);
    }
    
    /// <summary>
    /// Constructor with pagination and status filtering for "my events only".
    /// </summary>
    public EventsByOrganizerSpecification(Guid organizerId, EventStatus status, int skip, int take)
        : base(eventEntity => eventEntity.OrganizerId == organizerId && eventEntity.Status == status)
    {
        // Include pricing tiers by default for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users for invite-only events
        AddInclude(e => e.InvitedUsers);
        
        // Order by creation date descending (newest first)
        ApplyOrderByDescending(e => e.CreatedAt);
        
        // Apply pagination
        ApplyPaging(skip, take);
    }
}