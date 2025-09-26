using EventManagement.Web.Models;
using EventManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Web.Pages.Events;

public class CapacityModel : PageModel
{
    private readonly IEventApiService _eventApiService;
    private readonly ILogger<CapacityModel> _logger;

    public CapacityModel(IEventApiService eventApiService, ILogger<CapacityModel> logger)
    {
        _eventApiService = eventApiService;
        _logger = logger;
    }

    [BindProperty]
    public SetEventCapacityViewModel CapacityConfiguration { get; set; } = new();

    public EventViewModel Event { get; set; } = new();

    public bool IsError { get; set; }

    public string Message { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            // Get event details
            var eventResponse = await _eventApiService.GetEventByIdAsync(id);
            if (!eventResponse.Success || eventResponse.Data == null)
            {
                IsError = true;
                Message = eventResponse.Message ?? "Event not found";
                return Page();
            }

            Event = eventResponse.Data;

            // Only allow capacity configuration for Draft events
            if (Event.Status != EventStatus.Draft)
            {
                TempData["ErrorMessage"] = "Capacity and pricing can only be configured for draft events.";
                return RedirectToPage("/Events/Details", new { id = Event.Id });
            }

            // Load existing capacity configuration if available
            var capacityResponse = await _eventApiService.GetEventCapacityAndPricingAsync(id);
            
            if (capacityResponse.Success && capacityResponse.Data != null)
            {
                CapacityConfiguration = capacityResponse.Data;
            }
            else
            {
                // Initialize with default values
                CapacityConfiguration = new SetEventCapacityViewModel
                {
                    EventId = id,
                    TotalCapacity = 0,
                    PricingTiers = new List<PricingTierViewModel>()
                };
            }

            // Add a default pricing tier if none exist
            if (!CapacityConfiguration.PricingTiers.Any())
            {
                CapacityConfiguration.PricingTiers.Add(new PricingTierViewModel
                {
                    Name = "General Admission",
                    Price = 0,
                    Currency = "USD",
                    Capacity = 0,
                    SaleStartDate = DateTime.Now.Date,
                    SaleEndDate = Event.EndDate.Date
                });
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading capacity configuration for event {EventId}", id);
            IsError = true;
            Message = "An error occurred while loading the event capacity configuration.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Get event details again for validation
            var eventResponse = await _eventApiService.GetEventByIdAsync(CapacityConfiguration.EventId);
            if (!eventResponse.Success || eventResponse.Data == null)
            {
                ModelState.AddModelError(string.Empty, "Event not found");
                return Page();
            }

            Event = eventResponse.Data;

            // Validate business rules
            if (!ValidateCapacityConfiguration())
            {
                IsError = false; // Keep form visible for corrections
                return Page();
            }

            if (!ModelState.IsValid)
            {
                IsError = false;
                return Page();
            }

            // Save capacity configuration
            var response = await _eventApiService.SetEventCapacityAndPricingAsync(CapacityConfiguration);
            
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Event capacity and pricing have been configured successfully.";
                return RedirectToPage("/Events/Details", new { id = CapacityConfiguration.EventId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Failed to save capacity configuration");
                if (response.Errors?.Any() == true)
                {
                    foreach (var error in response.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                
                IsError = false;
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving capacity configuration for event {EventId}", CapacityConfiguration.EventId);
            ModelState.AddModelError(string.Empty, "An error occurred while saving the capacity configuration.");
            IsError = false;
            return Page();
        }
    }

    private bool ValidateCapacityConfiguration()
    {
        var isValid = true;

        // Validate total capacity
        if (CapacityConfiguration.TotalCapacity <= 0)
        {
            ModelState.AddModelError(nameof(CapacityConfiguration.TotalCapacity), "Total capacity must be greater than 0");
            isValid = false;
        }

        // Validate pricing tiers exist
        if (!CapacityConfiguration.PricingTiers.Any())
        {
            ModelState.AddModelError(nameof(CapacityConfiguration.PricingTiers), "At least one pricing tier is required");
            return false;
        }

        // Validate individual pricing tiers
        var totalTierCapacity = 0;
        for (int i = 0; i < CapacityConfiguration.PricingTiers.Count; i++)
        {
            var tier = CapacityConfiguration.PricingTiers[i];
            var prefix = $"CapacityConfiguration.PricingTiers[{i}]";

            if (string.IsNullOrWhiteSpace(tier.Name))
            {
                ModelState.AddModelError($"{prefix}.Name", "Tier name is required");
                isValid = false;
            }

            if (tier.Price < 0)
            {
                ModelState.AddModelError($"{prefix}.Price", "Price cannot be negative");
                isValid = false;
            }

            if (tier.Capacity <= 0)
            {
                ModelState.AddModelError($"{prefix}.Capacity", "Tier capacity must be greater than 0");
                isValid = false;
            }
            else
            {
                totalTierCapacity += tier.Capacity;
            }

            if (tier.SaleStartDate >= tier.SaleEndDate)
            {
                ModelState.AddModelError($"{prefix}.SaleEndDate", "Sale end date must be after start date");
                isValid = false;
            }

            if (tier.SaleEndDate > Event.EndDate)
            {
                ModelState.AddModelError($"{prefix}.SaleEndDate", "Sale end date cannot be after the event end date");
                isValid = false;
            }
        }

        // Validate capacity distribution
        if (totalTierCapacity != CapacityConfiguration.TotalCapacity)
        {
            ModelState.AddModelError(string.Empty, 
                $"The sum of all pricing tier capacities ({totalTierCapacity:N0}) must equal the total event capacity ({CapacityConfiguration.TotalCapacity:N0})");
            isValid = false;
        }

        return isValid;
    }

    // Helper method for revenue calculations
    public decimal GetMaximumRevenue()
    {
        return CapacityConfiguration.PricingTiers.Sum(pt => pt.Price * pt.Capacity);
    }

    public decimal GetMinimumPrice()
    {
        return CapacityConfiguration.PricingTiers.Any() ? CapacityConfiguration.PricingTiers.Min(pt => pt.Price) : 0;
    }

    public decimal GetMaximumPrice()
    {
        return CapacityConfiguration.PricingTiers.Any() ? CapacityConfiguration.PricingTiers.Max(pt => pt.Price) : 0;
    }

    public decimal GetAveragePrice()
    {
        if (!CapacityConfiguration.PricingTiers.Any()) return 0;
        
        var totalRevenue = CapacityConfiguration.PricingTiers.Sum(pt => pt.Price * pt.Capacity);
        var totalCapacity = CapacityConfiguration.PricingTiers.Sum(pt => pt.Capacity);
        
        return totalCapacity > 0 ? totalRevenue / totalCapacity : 0;
    }
}