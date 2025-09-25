using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventManagement.Web.Pages.Events;

public class CreateModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IEventApiService eventApiService, ILogger<CreateModel> logger)
    {
        _eventApiService = eventApiService;
        _logger = logger;
    }

    [BindProperty]
    public CreateEventViewModel Event { get; set; } = new();

    public List<VenueViewModel> Venues { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Set default date/time without seconds or milliseconds
        var defaultDateTime = DateTime.Now.AddDays(7);
        Event.Date = new DateTime(defaultDateTime.Year, defaultDateTime.Month, defaultDateTime.Day, 
                                 defaultDateTime.Hour, defaultDateTime.Minute, 0);
        
        await LoadVenues();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadVenues();
            return Page();
        }

        try
        {
            // Validate business rules
            if (Event.Date <= DateTime.Now)
            {
                ModelState.AddModelError("Event.Date", "Event date must be in the future.");
                await LoadVenues();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Event.Title) || Event.Title.Length < 3)
            {
                ModelState.AddModelError("Event.Title", "Event title must be at least 3 characters long.");
                await LoadVenues();
                return Page();
            }

            if (Event.Title.Length > 200)
            {
                ModelState.AddModelError("Event.Title", "Event title cannot exceed 200 characters.");
                await LoadVenues();
                return Page();
            }

            if (!string.IsNullOrEmpty(Event.Description) && Event.Description.Length > 1000)
            {
                ModelState.AddModelError("Event.Description", "Event description cannot exceed 1000 characters.");
                await LoadVenues();
                return Page();
            }

            var response = await _eventApiService.CreateEventAsync(Event);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToPage("/Events/List");
            }
            else
            {
                if (response.Errors.Any())
                {
                    foreach (var error in response.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            ModelState.AddModelError("", "An unexpected error occurred while creating the event.");
        }

        await LoadVenues();
        return Page();
    }

    private async Task LoadVenues()
    {
        try
        {
            var venuesResponse = await _eventApiService.GetVenuesAsync();
            Venues = venuesResponse.Success ? venuesResponse.Data ?? new List<VenueViewModel>() : new List<VenueViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading venues");
            Venues = new List<VenueViewModel>();
        }
    }
}