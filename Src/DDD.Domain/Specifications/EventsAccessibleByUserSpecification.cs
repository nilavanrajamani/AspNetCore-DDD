using System;
using System.Linq.Expressions;

using DDD.Domain.Models;

namespace DDD.Domain.Specifications;

/// <summary>
/// Specification to filter events accessible by a specific user.
/// This includes events the user organized, public events, and events they are invited to.
/// Implements the "filter my accessible events" business logic using the specification pattern.
/// </summary>
public class EventsAccessibleByUserSpecification : BaseSpecification<Event>
{
    public EventsAccessibleByUserSpecification(Guid userId)
        : base(BuildAccessibleEventsCriteria(userId))
    {
        // Include pricing tiers by default for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users to check invitation access
        AddInclude(e => e.InvitedUsers);
        
        // Order by creation date descending (newest first)
        ApplyOrderByDescending(e => e.CreatedAt);
    }
    
    /// <summary>
    /// Constructor with pagination support for accessible events filtering.
    /// </summary>
    public EventsAccessibleByUserSpecification(Guid userId, int skip, int take)
        : base(BuildAccessibleEventsCriteria(userId))
    {
        // Include pricing tiers by default for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users to check invitation access
        AddInclude(e => e.InvitedUsers);
        
        // Order by creation date descending (newest first)
        ApplyOrderByDescending(e => e.CreatedAt);
        
        // Apply pagination
        ApplyPaging(skip, take);
    }
    
    /// <summary>
    /// Builds the criteria expression for events accessible by a user.
    /// This includes:
    /// 1. Events organized by the user (my events)
    /// 2. Public events
    /// 3. Events where the user is invited
    /// </summary>
    private static Expression<Func<Event, bool>> BuildAccessibleEventsCriteria(Guid userId)
    {
        return eventEntity => 
            // Events organized by the user (my events)
            eventEntity.OrganizerId == userId ||
            
            // Public events (accessible to everyone)
            eventEntity.Visibility == EventVisibility.Public ||
            
            // Events where the user is invited
            eventEntity.InvitedUsers.Any(invite => invite.UserId == userId);
    }
}