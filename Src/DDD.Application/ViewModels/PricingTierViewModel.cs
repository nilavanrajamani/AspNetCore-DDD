using System;
using System.ComponentModel.DataAnnotations;

namespace DDD.Application.ViewModels;

public class PricingTierViewModel
{
    [Required]
    [StringLength(100, ErrorMessage = "Pricing tier name cannot exceed 100 characters")]
    public string Name { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code")]
    public string Currency { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public int Capacity { get; set; }

    [Required]
    public DateTime SaleStartDate { get; set; }

    [Required]
    public DateTime SaleEndDate { get; set; }
}