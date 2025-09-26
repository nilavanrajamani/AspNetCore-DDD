using System.ComponentModel.DataAnnotations;

namespace EventManagement.Web.Models;

// Capacity and Pricing ViewModels for US002
public class SetEventCapacityViewModel
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    [Range(1, 50000, ErrorMessage = "Total capacity must be between 1 and 50,000")]
    public int TotalCapacity { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one pricing tier is required")]
    public List<PricingTierViewModel> PricingTiers { get; set; } = new();
}

public class PricingTierViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Tier name must be between 1 and 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000, ErrorMessage = "Price must be between $0.01 and $10,000")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
    public string Currency { get; set; } = "USD";

    [Required]
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
    public int Capacity { get; set; }

    [Required]
    public DateTime SaleStartDate { get; set; }

    [Required]
    public DateTime SaleEndDate { get; set; }
}

public class PricingTierDefinitionViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Tier name must be between 1 and 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000, ErrorMessage = "Price must be between $0.01 and $10,000")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
    public string Currency { get; set; } = "USD";

    [Required]
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
    public int Capacity { get; set; }

    [Required]
    public DateTime SaleStartDate { get; set; }

    [Required]
    public DateTime SaleEndDate { get; set; }
}