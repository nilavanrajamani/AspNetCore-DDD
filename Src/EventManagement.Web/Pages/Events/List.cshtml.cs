using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventManagement.Web.Pages.Events;

public class ListModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ListModel> _logger;

    public ListModel(IEventApiService eventApiService, ICurrentUserService currentUserService, ILogger<ListModel> logger)
    {
        _eventApiService = eventApiService;
        _currentUserService = currentUserService;
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
            ApiResponse<List<EventViewModel>> response;
            
            // Use the new filtering API if "My Events Only" is checked
            if (ShowMyEventsOnly && _currentUserService.IsAuthenticated)
            {
                // Get current user ID and convert to Guid
                var userIdString = _currentUserService.UserId;
                if (Guid.TryParse(userIdString, out var currentUserId))
                {
                    // Use the appropriate filtering method based on status filter
                    if (StatusFilter.HasValue)
                    {
                        var statusString = StatusFilter.Value.ToString();
                        response = await _eventApiService.GetMyEventsOnlyAsync(currentUserId, statusString);
                    }
                    else
                    {
                        response = await _eventApiService.GetMyEventsOnlyAsync(currentUserId);
                    }
                }
                else
                {
                    // Fallback to regular API if user ID parsing fails
                    response = await _eventApiService.GetEventsAsync();
                    _logger.LogWarning("Failed to parse current user ID for filtering: {UserId}", userIdString);
                }
            }
            else
            {
                // Use regular API
                response = await _eventApiService.GetEventsAsync();
            }
            
            if (response.Success && response.Data != null)
            {
                var filteredEvents = response.Data.AsEnumerable();

                // Apply client-side search filter (since the API doesn't support search yet)
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filteredEvents = filteredEvents.Where(e => 
                        e.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (e.Description?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                // Apply client-side status filter if not already applied via API
                if (StatusFilter.HasValue && !ShowMyEventsOnly)
                {
                    filteredEvents = filteredEvents.Where(e => e.Status == StatusFilter.Value);
                }

                // Apply client-side "my events" filter if user is not authenticated or API filtering failed
                if (ShowMyEventsOnly && (!_currentUserService.IsAuthenticated || !Guid.TryParse(_currentUserService.UserId, out _)))
                {
                    if (_currentUserService.IsAuthenticated && Guid.TryParse(_currentUserService.UserId, out var fallbackUserId))
                    {
                        filteredEvents = filteredEvents.Where(e => e.OrganizerId == fallbackUserId);
                    }
                    else
                    {
                        // If not authenticated, show no events
                        filteredEvents = Enumerable.Empty<EventViewModel>();
                    }
                }

                EventList = new EventListViewModel
                {
                    Events = filteredEvents.OrderByDescending(e => e.CreatedAt).ToList(),
                    SearchTerm = SearchTerm,
                    StatusFilter = StatusFilter,
                    ShowMyEventsOnly = ShowMyEventsOnly,
                    Page = CurrentPage,
                    TotalCount = filteredEvents.Count(),
                };

                Message = response.Data.Any() ? string.Empty : "No events found.";
            }
            else
            {
                Message = response.Message ?? "Failed to load events.";
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