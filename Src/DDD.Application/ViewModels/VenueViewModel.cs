using System.ComponentModel.DataAnnotations;

namespace DDD.Application.ViewModels;

public class VenueViewModel
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The Name is Required")]
    [MinLength(2)]
    [MaxLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Address is Required")]
    [MinLength(5)]
    [MaxLength(500)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "The Capacity is Required")]
    [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
    [Display(Name = "Capacity")]
    public int Capacity { get; set; }
}
