using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DDD.Application.ViewModels;

public class SetEventCapacityViewModel
{
    public Guid EventId { get; set; }

    [Required]
    [Range(1, 50000, ErrorMessage = "Total capacity must be between 1 and 50,000")]
    public int TotalCapacity { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one pricing tier is required")]
    public List<PricingTierViewModel> PricingTiers { get; set; } = new();
}