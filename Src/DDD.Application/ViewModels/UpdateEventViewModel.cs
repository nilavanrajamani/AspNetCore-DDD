using System;
using System.ComponentModel.DataAnnotations;

namespace DDD.Application.ViewModels;

public class UpdateEventViewModel
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The Title is Required")]
    [MinLength(2)]
    [MaxLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Description")]
    public string Description { get; set; }

    [Required(ErrorMessage = "The Venue is Required")]
    [Display(Name = "Venue")]
    public Guid VenueId { get; set; }

    [Required(ErrorMessage = "The Start Date is Required")]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "The End Date is Required")]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Force Update")]
    public bool ForceUpdate { get; set; }
}