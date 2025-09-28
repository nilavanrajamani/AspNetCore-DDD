using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventManagement.Web.Pages.Events;

public class VisibilityModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ILogger<VisibilityModel> _logger;

    public VisibilityModel(IEventApiService eventApiService, ILogger<VisibilityModel> logger)
    {
        _eventApiService = eventApiService;
        _logger = logger;
    }

    public EventViewModel Event { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
    
    [BindProperty]
    public Guid EventId { get; set; }
    
    [BindProperty]
    public string SelectedVisibility { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            var response = await _eventApiService.GetEventByIdAsync(id);
            
            if (response.Success && response.Data != null)
            {
                Event = response.Data;
                EventId = id;
                SelectedVisibility = Event.Visibility.ToString();
            }
            else
            {
                Message = response.Message;
                IsError = true;
                
                if (response.Message.Contains("not found"))
                {
                    return NotFound();
                }
            }
        }
        catch (Exception ex)
        {
            Message = "An error occurred while loading the event.";
            IsError = true;
            _logger.LogError(ex, "Error loading event details for visibility management {EventId}", id);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (EventId == Guid.Empty)
        {
            TempData["Error"] = "Invalid event ID.";
            return RedirectToPage("./List");
        }

        if (string.IsNullOrWhiteSpace(SelectedVisibility))
        {
            TempData["Error"] = "Please select a visibility option.";
            return RedirectToPage("./Visibility", new { id = EventId });
        }

        try
        {
            // Validate visibility value
            if (!Enum.TryParse<EventVisibility>(SelectedVisibility, out var visibility))
            {
                TempData["Error"] = "Invalid visibility option selected.";
                return RedirectToPage("./Visibility", new { id = EventId });
            }

            var response = await _eventApiService.SetEventVisibilityAsync(EventId, SelectedVisibility);
            
            if (response.Success)
            {
                TempData["Success"] = $"Event visibility updated to {SelectedVisibility} successfully.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while updating event visibility.";
            _logger.LogError(ex, "Error updating event visibility for {EventId} to {Visibility}", EventId, SelectedVisibility);
        }

        return RedirectToPage("./Visibility", new { id = EventId });
    }

    public async Task<IActionResult> OnPostInviteUserAsync(Guid eventId, string email, string role)
    {
        if (eventId == Guid.Empty || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
        {
            TempData["Error"] = "Invalid invitation data provided.";
            return RedirectToPage("./Visibility", new { id = eventId });
        }

        try
        {
            // For now, we'll mock the user lookup and invitation process
            // In a real implementation, this would:
            // 1. Look up the user by email
            // 2. Call the API service to send the invitation
            
            // Mock implementation
            var mockUserId = Guid.NewGuid(); // This would come from user lookup
            var response = await _eventApiService.InviteUserToEventAsync(eventId, mockUserId, role);
            
            if (response.Success)
            {
                TempData["Success"] = $"Invitation sent to {email} successfully.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while sending the invitation.";
            _logger.LogError(ex, "Error sending invitation to {Email} for event {EventId}", email, eventId);
        }

        return RedirectToPage("./Visibility", new { id = eventId });
    }
}