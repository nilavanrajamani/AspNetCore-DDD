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
    public DateTime Date { get; set; }
    public EventStatus Status { get; set; }
    public EventVisibility Visibility { get; set; }
    public Guid OrganizerId { get; set; }
    public Guid? VenueId { get; set; }
    public string? VenueName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEventViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public EventVisibility Visibility { get; set; } = EventVisibility.Public;
    public Guid? VenueId { get; set; }
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