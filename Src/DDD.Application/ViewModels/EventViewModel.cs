using System;
using System.ComponentModel.DataAnnotations;

namespace DDD.Application.ViewModels;

public class EventViewModel
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The Title is Required")]
    [MinLength(2)]
    [MaxLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; }

    [Required(ErrorMessage = "The Description is Required")]
    [MinLength(10)]
    [MaxLength(2000)]
    [Display(Name = "Description")]
    public string Description { get; set; }

    [Required(ErrorMessage = "The Organizer ID is Required")]
    [Display(Name = "Organizer ID")]
    public Guid OrganizerId { get; set; }

    [Required(ErrorMessage = "The Venue ID is Required")]
    [Display(Name = "Venue ID")]
    public Guid VenueId { get; set; }

    [Required(ErrorMessage = "The Start Date is Required")]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "The End Date is Required")]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; }

    [Display(Name = "Visibility")]
    public string Visibility { get; set; }
}