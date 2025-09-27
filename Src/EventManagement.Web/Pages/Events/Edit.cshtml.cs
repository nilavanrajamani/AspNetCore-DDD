using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventManagement.Web.Pages.Events;

public class EditModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IEventApiService eventApiService, ILogger<EditModel> logger)
    {
        _eventApiService = eventApiService;
        _logger = logger;
    }

    [BindProperty]
    public UpdateEventViewModel UpdateEventModel { get; set; } = new();

    public EventViewModel Event { get; set; } = new();
    public List<VenueViewModel> Venues { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            // Get the event details
            var eventResponse = await _eventApiService.GetEventByIdAsync(id);
            if (!eventResponse.Success || eventResponse.Data == null)
            {
                TempData["Error"] = eventResponse.Message ?? "Event not found.";
                return RedirectToPage("/Events/List");
            }

            Event = eventResponse.Data;

            // Get venues for the dropdown
            var venuesResponse = await _eventApiService.GetVenuesAsync();
            if (venuesResponse.Success && venuesResponse.Data != null)
            {
                Venues = venuesResponse.Data;
            }
            else
            {
                Venues = new List<VenueViewModel>();
                _logger.LogWarning("Failed to load venues: {Message}", venuesResponse.Message);
            }

            // Initialize the update model with current event data
            UpdateEventModel = new UpdateEventViewModel
            {
                Id = Event.Id,
                Title = Event.Title,
                Description = Event.Description,
                VenueId = Event.VenueId ?? Guid.Empty,
                StartDate = Event.StartDate,
                EndDate = Event.EndDate,
                ForceUpdate = false
            };

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event {EventId} for editing", id);
            TempData["Error"] = "An error occurred while loading the event. Please try again.";
            return RedirectToPage("/Events/List");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Reload event and venues for the form in case of validation errors
            var eventResponse = await _eventApiService.GetEventByIdAsync(UpdateEventModel.Id);
            if (eventResponse.Success && eventResponse.Data != null)
            {
                Event = eventResponse.Data;
            }

            var venuesResponse = await _eventApiService.GetVenuesAsync();
            if (venuesResponse.Success && venuesResponse.Data != null)
            {
                Venues = venuesResponse.Data;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Additional server-side validation
            if (UpdateEventModel.EndDate <= UpdateEventModel.StartDate)
            {
                ModelState.AddModelError("UpdateEventModel.EndDate", "End date must be after start date.");
                return Page();
            }

            if (UpdateEventModel.StartDate <= DateTime.Now.AddHours(-1)) // Allow 1 hour buffer
            {
                ModelState.AddModelError("UpdateEventModel.StartDate", "Start date must be in the future.");
                return Page();
            }

            // Validate venue selection
            if (UpdateEventModel.VenueId == Guid.Empty)
            {
                ModelState.AddModelError("UpdateEventModel.VenueId", "Please select a venue.");
                return Page();
            }

            // Check if this is a critical change to a published event
            if (Event.Status == EventStatus.Published && !UpdateEventModel.ForceUpdate)
            {
                bool isCriticalChange = false;

                // Check for date changes
                if (Event.StartDate != UpdateEventModel.StartDate || Event.EndDate != UpdateEventModel.EndDate)
                {
                    isCriticalChange = true;
                }

                // Check for venue changes
                if (Event.VenueId != UpdateEventModel.VenueId)
                {
                    isCriticalChange = true;
                }

                if (isCriticalChange)
                {
                    ModelState.AddModelError(string.Empty, 
                        "You are attempting to make critical changes to a published event. " +
                        "Please check 'Force update critical changes' to confirm this action.");
                    return Page();
                }
            }

            // Call the API to update the event
            var updateResponse = await _eventApiService.UpdateEventDetailsAsync(UpdateEventModel);

            if (updateResponse.Success)
            {
                TempData["Success"] = "Event details updated successfully!";
                return RedirectToPage("/Events/Details", new { id = UpdateEventModel.Id });
            }
            else
            {
                TempData["Error"] = updateResponse.Message ?? "Failed to update event details.";
                
                // Add specific errors to ModelState if available
                if (updateResponse.Errors?.Any() == true)
                {
                    foreach (var error in updateResponse.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", UpdateEventModel.Id);
            TempData["Error"] = "An unexpected error occurred while updating the event. Please try again.";
            return Page();
        }
    }
}