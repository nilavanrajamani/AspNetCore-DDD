using System.ComponentModel.DataAnnotations;

namespace DDD.Application.ViewModels;

/// <summary>
/// View model for pricing tier definition.
/// </summary>
public class PricingTierDefinitionViewModel
{
    /// <summary>
    /// Gets or sets the tier name.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Tier name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price for this tier.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the capacity allocated to this tier.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public int Capacity { get; set; }
}