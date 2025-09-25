using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventManagement.Web.Pages.Events;

public class ListModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ILogger<ListModel> _logger;

    public ListModel(IEventApiService eventApiService, ILogger<ListModel> logger)
    {
        _eventApiService = eventApiService;
        _logger = logger;
    }

    public EventListViewModel EventList { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public EventStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ShowMyEventsOnly { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public async Task OnGetAsync()
    {
        try
        {
            var response = await _eventApiService.GetEventsAsync();
            
            if (response.Success && response.Data != null)
            {
                var filteredEvents = response.Data.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filteredEvents = filteredEvents.Where(e => 
                        e.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (e.Description?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                // Apply status filter
                if (StatusFilter.HasValue)
                {
                    filteredEvents = filteredEvents.Where(e => e.Status == StatusFilter.Value);
                }

                // Apply my events filter (placeholder - in real implementation, filter by current user)
                if (ShowMyEventsOnly)
                {
                    // TODO: Filter by current user's OrganizerId
                    // filteredEvents = filteredEvents.Where(e => e.OrganizerId == currentUserId);
                }

                EventList = new EventListViewModel
                {
                    Events = filteredEvents.OrderByDescending(e => e.CreatedAt).ToList(),
                    SearchTerm = SearchTerm,
                    StatusFilter = StatusFilter,
                    ShowMyEventsOnly = ShowMyEventsOnly,
                    Page = CurrentPage,
                    TotalCount = filteredEvents.Count()
                };

                Message = response.Data.Any() ? "" : "No events found.";
            }
            else
            {
                Message = response.Message;
                IsError = true;
                _logger.LogWarning("Failed to fetch events: {Message}", response.Message);
            }
        }
        catch (Exception ex)
        {
            Message = "An error occurred while loading events.";
            IsError = true;
            _logger.LogError(ex, "Error loading events list");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var response = await _eventApiService.DeleteEventAsync(id);
            
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Event deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the event.";
            _logger.LogError(ex, "Error deleting event {EventId}", id);
        }

        return RedirectToPage();
    }
}