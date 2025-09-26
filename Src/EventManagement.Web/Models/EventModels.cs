namespace EventManagement.Web.Models;

public enum EventStatus
{
    Draft = 0,
    Published = 1,
    Cancelled = 2,
    Completed = 3
}

public enum EventVisibility
{
    Public = 0,
    Private = 1,
    InviteOnly = 2
}

public class EventViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatus Status { get; set; }
    public EventVisibility Visibility { get; set; }
    public Guid OrganizerId { get; set; }
    public Guid? VenueId { get; set; }
    public string? VenueName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Capacity and Pricing Properties
    public int? TotalCapacity { get; set; }
    public List<PricingTierViewModel> PricingTiers { get; set; } = new();
    
    // Helper property for backward compatibility and display purposes
    public DateTime Date 
    { 
        get => StartDate; 
        set => StartDate = value; 
    }
}

public class CreateEventViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventVisibility Visibility { get; set; } = EventVisibility.Public;
    public Guid? VenueId { get; set; }
    
    // Helper property for backward compatibility
    public DateTime Date 
    { 
        get => StartDate; 
        set => StartDate = value; 
    }
}

public class VenueViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class EventListViewModel
{
    public List<EventViewModel> Events { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public EventStatus? StatusFilter { get; set; }
    public bool ShowMyEventsOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
}