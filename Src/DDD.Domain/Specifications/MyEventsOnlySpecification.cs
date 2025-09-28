using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using DDD.Domain.Models;

namespace DDD.Domain.Specifications;

/// <summary>
/// Specification for filtering "my events only" with advanced criteria.
/// This implements the core business requirement to filter events that belong specifically
/// to the current user/organizer with various filtering options.
/// </summary>
public class MyEventsOnlySpecification : BaseSpecification<Event>
{
    /// <summary>
    /// Basic "my events only" filter by organizer ID.
    /// </summary>
    public MyEventsOnlySpecification(Guid organizerId)
        : base(eventEntity => eventEntity.OrganizerId == organizerId)
    {
        ConfigureDefaultIncludes();
        ApplyDefaultOrdering();
    }
    
    /// <summary>
    /// "My events only" with status filtering.
    /// </summary>
    public MyEventsOnlySpecification(Guid organizerId, EventStatus status)
        : base(eventEntity => eventEntity.OrganizerId == organizerId && eventEntity.Status == status)
    {
        ConfigureDefaultIncludes();
        ApplyDefaultOrdering();
    }
    
    /// <summary>
    /// "My events only" with multiple status filtering.
    /// </summary>
    public MyEventsOnlySpecification(Guid organizerId, IEnumerable<EventStatus> statuses)
        : base(BuildMultipleStatusCriteria(organizerId, statuses))
    {
        ConfigureDefaultIncludes();
        ApplyDefaultOrdering();
    }
    
    /// <summary>
    /// "My events only" with date range filtering.
    /// </summary>
    public MyEventsOnlySpecification(Guid organizerId, DateTime startDate, DateTime endDate)
        : base(eventEntity => eventEntity.OrganizerId == organizerId && 
                             eventEntity.StartDate >= startDate && 
                             eventEntity.StartDate <= endDate)
    {
        ConfigureDefaultIncludes();
        ApplyDefaultOrdering();
    }
    
    /// <summary>
    /// "My events only" with comprehensive filtering options.
    /// </summary>
    public MyEventsOnlySpecification(
        Guid organizerId, 
        EventStatus? status = null,
        EventVisibility? visibility = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? skip = null,
        int? take = null)
        : base(BuildComprehensiveCriteria(organizerId, status, visibility, startDate, endDate))
    {
        ConfigureDefaultIncludes();
        ApplyDefaultOrdering();
        
        // Apply pagination if specified
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }
    
    /// <summary>
    /// Configures the default includes for event entities.
    /// </summary>
    private void ConfigureDefaultIncludes()
    {
        // Include pricing tiers for complete event information
        AddInclude(e => e.PricingTiers);
        
        // Include invited users for invitation management
        AddInclude(e => e.InvitedUsers);
    }
    
    /// <summary>
    /// Applies the default ordering (newest events first).
    /// </summary>
    private void ApplyDefaultOrdering()
    {
        ApplyOrderByDescending(e => e.CreatedAt);
    }
    
    /// <summary>
    /// Builds criteria for filtering by multiple event statuses.
    /// </summary>
    private static Expression<Func<Event, bool>> BuildMultipleStatusCriteria(Guid organizerId, IEnumerable<EventStatus> statuses)
    {
        var statusList = statuses.ToList();
        return eventEntity => eventEntity.OrganizerId == organizerId && statusList.Contains(eventEntity.Status);
    }
    
    /// <summary>
    /// Builds comprehensive filtering criteria with all optional parameters.
    /// </summary>
    private static Expression<Func<Event, bool>> BuildComprehensiveCriteria(
        Guid organizerId,
        EventStatus? status,
        EventVisibility? visibility,
        DateTime? startDate,
        DateTime? endDate)
    {
        return eventEntity => 
            // Must be organized by the user (core "my events only" requirement)
            eventEntity.OrganizerId == organizerId &&
            
            // Optional status filter
            (!status.HasValue || eventEntity.Status == status.Value) &&
            
            // Optional visibility filter
            (!visibility.HasValue || eventEntity.Visibility == visibility.Value) &&
            
            // Optional start date filter
            (!startDate.HasValue || eventEntity.StartDate >= startDate.Value) &&
            
            // Optional end date filter
            (!endDate.HasValue || eventEntity.StartDate <= endDate.Value);
    }
}