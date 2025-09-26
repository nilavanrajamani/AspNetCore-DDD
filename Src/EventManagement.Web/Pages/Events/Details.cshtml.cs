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

    public async Task<IActionResult> OnPostPublishAsync(Guid id)
    {
        try
        {
            var response = await _eventApiService.PublishEventAsync(id);
            
            if (response.Success)
            {
                TempData["Success"] = response.Message;
            }
            else
            {
                TempData["Error"] = response.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while publishing the event.";
            _logger.LogError(ex, "Error publishing event {EventId}", id);
        }

        return RedirectToPage("./Details", new { id });
    }

    public async Task<IActionResult> OnPostUnpublishAsync(Guid id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "A reason is required to unpublish an event.";
            return RedirectToPage("./Details", new { id });
        }

        try
        {
            var response = await _eventApiService.UnpublishEventAsync(id, reason);
            
            if (response.Success)
            {
                TempData["Success"] = response.Message;
            }
            else
            {
                TempData["Error"] = response.Message;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while unpublishing the event.";
            _logger.LogError(ex, "Error unpublishing event {EventId}", id);
        }

        return RedirectToPage("./Details", new { id });
    }
}