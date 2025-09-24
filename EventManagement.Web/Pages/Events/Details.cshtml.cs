using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventManagement.Web.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IEventApiService eventApiService, ILogger<DetailsModel> logger)
    {
        _eventApiService = eventApiService;
        _logger = logger;
    }

    public EventViewModel Event { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }

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
            _logger.LogError(ex, "Error loading event details for {EventId}", id);
        }

        return Page();
    }
}